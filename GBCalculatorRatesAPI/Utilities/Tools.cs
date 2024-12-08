namespace GBCalculatorRatesAPI.Utilities;

public static class Tools {

    public static string GetCacheType<T>() {
        return typeof(T).Name;
    }
}