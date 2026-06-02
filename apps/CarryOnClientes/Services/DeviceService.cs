using System;
using System.IO;

namespace BarPedidos.Services
{
    // Serviço que gere o identificador único de cada smartphone
    public class DeviceService
    {
        // Nome do ficheiro onde o ID fica guardado no telemóvel
        private const string DEVICE_ID_FILE = "device_id.txt";

        // Variável privada que guarda o ID único deste dispositivo
        private string _deviceId;

        // Obtém ou cria o ID único logo na inicialização
        public DeviceService()
        {
            _deviceId = GetOrCreateDeviceId();
        }

        // Método público que retorna o ID único deste smartphone
        // Outros serviços chamam este método para saber qual é o ID do dispositivo
        // Ex: FirebaseService usa isto para filtrar pedidos
        public string GetDeviceId()
        {
            return _deviceId;
        }

        // Método privado que cria ou lê o ID guardado
        // Se for a primeira vez, cria um ID novo e guarda
        // Se já existir, lê o ID guardado anteriormente
        private string GetOrCreateDeviceId()
        {
            // Combina o caminho da pasta de dados da app com o nome do ficheiro
            string filePath = Path.Combine(FileSystem.AppDataDirectory, DEVICE_ID_FILE);

            // Verifica se o ficheiro já existe no dispositivo
            if (File.Exists(filePath))
            {
                // Ficheiro existe: lê o ID que estava guardado para que seja sempre o mesmo entre sessões
                return File.ReadAllText(filePath);
            }

            // Ficheiro não existe: é a primeira vez que a app abre neste dispositivo
            // Cria um novo ID único
            string newId = Guid.NewGuid().ToString();

            // Guarda o novo ID no ficheiro para usar nas próximas vezes
            File.WriteAllText(filePath, newId);

            // Retorna o ID recém-criado
            return newId;
        }


        // Remove o ficheiro e cria um novo ID
        // Nota: normalmente não é usado, serve para debugging
        public void ResetDeviceId()
        {
            // Constrói o caminho completo do ficheiro
            string filePath = Path.Combine(FileSystem.AppDataDirectory, DEVICE_ID_FILE);

            // Se o ficheiro existir, apaga-o
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Cria um novo ID (como se fosse a primeira vez)
            _deviceId = GetOrCreateDeviceId();
        }
    }
}
