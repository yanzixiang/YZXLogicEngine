using Eneter.Messaging.DataProcessing.Serializing;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace YZXMessaging
{
  public class YAMLSerializer:ISerializer
  {
  #region ISerializer 成员

    public _T Deserialize<_T>(object serializedData)
    {
      string s = (string)serializedData;
      _T t = JsonConvert.DeserializeObject<_T>(s);
      return (_T)t;
    }

    public object Serialize<_T>(_T dataToSerialize)
    {
      string s = JsonConvert.SerializeObject(dataToSerialize);
      return s;
    }

    #endregion
  }
}
