using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SummerGameServer.Models;
using SummerGameServer.Models.VOs;

namespace SummerGameServer.Services
{
    public class CatalogManager
    {
        private IReadOnlyDictionary<Type, ICatalog> _catalogDic;
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };
        public CatalogManager(IReadOnlyDictionary<Type,ICatalog> catalogDic)
        {
            _catalogDic = catalogDic;
        }
        public T? GetCatalogModel<T>(int id) where T : ICatalogModel 
        {
            Type t = typeof(T);
            ModelCatalog<T>? catalog = _catalogDic.GetValueOrDefault(t) as ModelCatalog<T>;
            if (catalog == null)
                return default(T);
            return catalog.FindCatalog(id);
        }
        public static CatalogManager LoadFrom(string contentRoot)
        {
            string dir = Path.Combine(contentRoot, "GameData");
            Dictionary<Type, ICatalog> catalogs = new();
            catalogs.Add(typeof(MapVO), MakeCatalog<MapVO>(Path.Combine(dir, "Maps"), "Map"));
            catalogs.Add(typeof(StageVO),MakeCatalog<StageVO>(Path.Combine(dir, "Stages"), "Stage"));
            //나중에 Trap들도 추가

            return new CatalogManager(catalogs);
        }
        private static ICatalog MakeCatalog<T>(string directoryPath, string filePrefix) where T : ICatalogModel
        {
            Dictionary<int, T> models = ReadAllJson<T>(directoryPath, filePrefix).ToDictionary(elem => elem.Id);
            return new ModelCatalog<T>(models);
        }
        private static List<T> ReadAllJson<T>(string directoryPath, string filePrefix)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException(
                    $"마스터 데이터 폴더를 찾을 수 없습니다. : {directoryPath}");

            if (string.IsNullOrWhiteSpace(filePrefix))
                throw new ArgumentException(
                    "파일 접두사는 비어 있을 수 없습니다.",
                    nameof(filePrefix));

            string[] filePaths = Directory
                .EnumerateFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly)
                .Where(path => Path
                    .GetFileNameWithoutExtension(path)
                    .StartsWith(filePrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(path =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string numberText = fileName[filePrefix.Length..];

                    return int.TryParse(numberText, out int number)
                        ? number
                        : int.MaxValue;
                })
                .ThenBy(
                    Path.GetFileName,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (filePaths.Length == 0)
                throw new FileNotFoundException(
                    $"'{filePrefix}*.json' 파일을 찾을 수 없습니다. : {directoryPath}");

            List<T> models = new(filePaths.Length);

            foreach (string filePath in filePaths)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    T model = JsonConvert.DeserializeObject<T>(json, JsonSettings)
                        ?? throw new InvalidDataException(
                            $"마스터 데이터 파싱 결과가 null입니다. : {filePath}");

                    models.Add(model);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    throw new InvalidDataException(
                        $"마스터 데이터 JSON 형식이 올바르지 않습니다. : {filePath}",
                        exception);
                }
            }

            return models;
        }

    }
}
