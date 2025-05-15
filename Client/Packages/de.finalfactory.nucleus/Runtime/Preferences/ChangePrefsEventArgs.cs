namespace FinalFactory.Preferences
{
    public class ChangePrefsEventArgs
    {
        public string Key { get; }
        public object Value { get; }
        
        public bool IsEncrypted { get; }

        public ChangePrefsEventArgs(string key, object value, bool isEncrypted)
        {
            Key = key;
            Value = value;
            IsEncrypted = isEncrypted;
        }
    }
}