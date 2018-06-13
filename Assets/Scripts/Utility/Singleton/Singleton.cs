public class Singleton<T> where T : new()
{
    private static Singleton<T> _instance;
    public static Singleton<T> Instance {
    get {
            if (_instance == null) {
                _instance = new Singleton<T>();
            }
            return _instance;
        }
    }

    public virtual void Release() { }
}