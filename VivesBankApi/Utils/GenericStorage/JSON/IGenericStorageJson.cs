namespace VivesBankApi.Utils.GenericStorage.JSON;

public interface IGenericStorageJson<T> where T : class
{
    IObservable<T> Import(IFormFile fileStream);
    Task<FileStream> Export(List<T> entities);
}