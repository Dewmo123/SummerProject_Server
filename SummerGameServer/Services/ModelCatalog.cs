using SummerGameServer.Models.VOs;

namespace SummerGameServer.Services
{
    public interface ICatalog
    {

    }
    public class ModelCatalog<T> : ICatalog where T : ICatalogModel
    {
        private IReadOnlyDictionary<int, T> _models;
        public ModelCatalog(IReadOnlyDictionary<int, T> models)
        {
            _models = models;
        }
        public T? FindCatalog(int id) => _models.GetValueOrDefault(id);
    }
}
