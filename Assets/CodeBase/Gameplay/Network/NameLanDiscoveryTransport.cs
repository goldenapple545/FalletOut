using System;
using System.IO;
using System.Net;
using System.Text;
using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using Zenject;

namespace CodeBase.Gameplay.Network
{
    public class NameLanDiscoveryTransport : LanDiscoveryTransport
    {
        private IStaticDataService _staticDataService;

        [Inject]
        private void Construct(IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService;
        }

        private int MaxPlayers =>
            _staticDataService != null
                ? _staticDataService.MatchRulesConfig.MaxPlayers
                : 6;

        public string AdvertisedName    { get; private set; } = "Сервер 1";
        public int    AdvertisedPlayers { get; private set; } = 1;

        public event Action<ServerInfo> ServerInfoFound;

        public void SetAdvertisement(string serverName, int currentPlayers)
        {
            AdvertisedName    = serverName;
            AdvertisedPlayers = currentPlayers;
        }

        // Сервер → клиент: байт 1 + currentPlayers + maxPlayers + имя
        protected override byte[] GetAdvertisementData()
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write((byte)1);
            bw.Write(AdvertisedPlayers);
            bw.Write(MaxPlayers);

            byte[] nameBytes = Encoding.UTF8.GetBytes(AdvertisedName);
            bw.Write(nameBytes.Length);
            bw.Write(nameBytes);

            bw.Flush();
            return ms.ToArray();
        }

        // Клиент разбирает расширенный пакет
        protected override void OnServerResponseReceived(IPEndPoint endPoint, byte[] data)
        {
            try
            {
                using var ms = new MemoryStream(data);
                using var br = new BinaryReader(ms);

                br.ReadByte(); // ok byte
                int current = br.ReadInt32();
                int max     = br.ReadInt32();
                int nameLen = br.ReadInt32();
                string name = Encoding.UTF8.GetString(br.ReadBytes(nameLen));

                ServerInfoFound?.Invoke(new ServerInfo(endPoint, name, current, max));
            }
            catch
            {
                // Если пакет не расширенный — фолбек к базовому поведению
                base.OnServerResponseReceived(endPoint, data);
            }
        }
    }
}