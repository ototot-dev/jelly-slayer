
/*     ===== Do not touch this. Auto Generated Code. =====    */
/*     If you want custom code generation modify this => 'CodeGeneratorUnityEngine.cs'  */
using GoogleSheet.Protocol.v2.Res;
using GoogleSheet.Protocol.v2.Req;
using UGS;
using System;
using UGS.IO;
using GoogleSheet;
using System.Collections.Generic;
using System.IO;
using GoogleSheet.Type;
using System.Reflection;
using UnityEngine;
using Game;


namespace MainTable
{
    [GoogleSheet.Attribute.TableStruct]
    public partial class PawnData : ITable
    { 

        public delegate void OnLoadedFromGoogleSheets(List<PawnData> loadedList, Dictionary<int, PawnData> loadedDictionary);

        static bool isLoaded = false;
        static string spreadSheetID = "19oz9FuD0eLqBOgKr9hycuWc0LfzEbMXmKqJc-HrEWWI"; // it is file id
        static string sheetID = "0"; // it is sheet id
        static UnityFileReader reader = new UnityFileReader();

/* Your Loaded Data Storage. */
    
        public static Dictionary<int, PawnData> PawnDataMap = new Dictionary<int, PawnData>();  
        public static List<PawnData> PawnDataList = new List<PawnData>();   

        /// <summary>
        /// Get PawnData List 
        /// Auto Load
        /// </summary>
        public static List<PawnData> GetList()
        {{
           if (isLoaded == false) Load();
           return PawnDataList;
        }}

        /// <summary>
        /// Get PawnData Dictionary, keyType is your sheet A1 field type.
        /// - Auto Load
        /// </summary>
        public static Dictionary<int, PawnData>  GetDictionary()
        {{
           if (isLoaded == false) Load();
           return PawnDataMap;
        }}

    

/* Fields. */

		public System.Int32 index;
		public PawnId pawnId;
		public System.Int32 health;
		public System.Int32 actionPoint;
		public System.Int32 stamina;
		public System.Int32 stance;
		public System.Int32 knockDown;
		public System.Int32 rage;
		public System.Int32 poise;
		public System.Int32 physAttack;
		public System.Int32 magicAttack;
		public System.Int32 physDefence;
		public System.Int32 magicDefence;
		public System.Int32 guardStrength;
		public System.Single guardEfficiency;
		public System.Single guardStaminaCost;
		public System.Single guardStaggerDuration;
		public System.Single staminaRecoverSpeed;
		public System.Single staminaRecoverTime;
		public System.Int32 groggyHitCount;
		public System.Single groggyDuration;
		public System.Single knockDownDuration;
		public System.String name;
		public System.String prefPath;
  

#region fuctions


        public static void Load(bool forceReload = false)
        {
            if(isLoaded && forceReload == false)
            {
#if UGS_DEBUG
                 Debug.Log("PawnData is already loaded! if you want reload then, forceReload parameter set true");
#endif
                 return;
            }

            string text = reader.ReadData("MainTable"); 
            if (text != null)
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ReadSpreadSheetResult>(text);
                CommonLoad(result.jsonObject, forceReload); 
                if(!isLoaded)isLoaded = true;
            }
      
        }
 

        public static void LoadFromGoogle(System.Action<List<PawnData>, Dictionary<int, PawnData>> onLoaded, bool updateCurrentData = false)
        {      
                IHttpProtcol webInstance = null;
    #if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    webInstance = UnityEditorWebRequest.Instance as IHttpProtcol;
                }
                else 
                {
                    webInstance = UnityPlayerWebRequest.Instance as IHttpProtcol;
                }
    #endif
    #if !UNITY_EDITOR
                     webInstance = UnityPlayerWebRequest.Instance as IHttpProtcol;
    #endif
          
 
                var mdl = new ReadSpreadSheetReqModel(spreadSheetID);
                webInstance.ReadSpreadSheet(mdl, OnError, (data) => {
                    var loaded = CommonLoad(data.jsonObject, updateCurrentData); 
                    onLoaded?.Invoke(loaded.list, loaded.map);
                });
        }

               


    public static (List<PawnData> list, Dictionary<int, PawnData> map) CommonLoad(Dictionary<string, Dictionary<string, List<string>>> jsonObject, bool forceReload){
            Dictionary<int, PawnData> Map = new Dictionary<int, PawnData>();
            List<PawnData> List = new List<PawnData>();     
            TypeMap.Init();
            FieldInfo[] fields = typeof(PawnData).GetFields(BindingFlags.Public | BindingFlags.Instance);
            List<(string original, string propertyName, string type)> typeInfos = new List<(string, string, string)>(); 
            List<List<string>> rows = new List<List<string>>();
            var sheet = jsonObject["PawnData"];

            foreach (var column in sheet.Keys)
            {
                string[] split = column.Replace(" ", null).Split(':');
                         string column_field = split[0];
                string   column_type = split[1];

                typeInfos.Add((column, column_field, column_type));
                          List<string> typeValues = sheet[column];
                rows.Add(typeValues);
            }

          // 실제 데이터 로드
                    if (rows.Count != 0)
                    {
                        int rowCount = rows[0].Count;
                        for (int i = 0; i < rowCount; i++)
                        {
                            PawnData instance = new PawnData();
                            for (int j = 0; j < typeInfos.Count; j++)
                            {
                                try
                                {
                                    var typeInfo = TypeMap.StrMap[typeInfos[j].type];
                                    //int, float, List<..> etc
                                    string type = typeInfos[j].type;
                                    if (type.StartsWith(" < ") && type.Substring(1, 4) == "Enum" && type.EndsWith(">"))
                                    {
                                         Debug.Log("It's Enum");
                                    }

                                    var readedValue = TypeMap.Map[typeInfo].Read(rows[j][i]);
                                    fields[j].SetValue(instance, readedValue);

                                }
                                catch (Exception e)
                                {
                                    if (e is UGSValueParseException)
                                    {
                                        Debug.LogError("<color=red> UGS Value Parse Failed! </color>");
                                        Debug.LogError(e);
                                        return (null, null);
                                    }

                                    //enum parse
                                    var type = typeInfos[j].type;
                                    type = type.Replace("Enum<", null);
                                    type = type.Replace(">", null);

                                    var readedValue = TypeMap.EnumMap[type].Read(rows[j][i]);
                                    fields[j].SetValue(instance, readedValue); 
                                }
                              
                            }
                            List.Add(instance); 
                            Map.Add(instance.index, instance);
                        }
                        if(isLoaded == false || forceReload)
                        { 
                            PawnDataList = List;
                            PawnDataMap = Map;
                            isLoaded = true;
                        }
                    } 
                    return (List, Map); 
}


 

        public static void Write(PawnData data, System.Action<WriteObjectResult> onWriteCallback = null)
        { 
            TypeMap.Init();
            FieldInfo[] fields = typeof(PawnData).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var datas = new string[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var type = fields[i].FieldType;
                string writeRule = null;
                if(type.IsEnum)
                {
                    writeRule = TypeMap.EnumMap[type.Name].Write(fields[i].GetValue(data));
                }
                else
                {
                    writeRule = TypeMap.Map[type].Write(fields[i].GetValue(data));
                } 
                datas[i] = writeRule; 
            }  
           
#if UNITY_EDITOR
if(Application.isPlaying == false)
{
                UnityPlayerWebRequest.Instance.WriteObject(new WriteObjectReqModel(spreadSheetID, sheetID, datas[0], datas), OnError, onWriteCallback);

}
else
{
            UnityPlayerWebRequest.Instance.WriteObject(new  WriteObjectReqModel(spreadSheetID, sheetID, datas[0], datas), OnError, onWriteCallback);

}
#endif

#if !UNITY_EDITOR
   UnityPlayerWebRequest.Instance.WriteObject(new  WriteObjectReqModel(spreadSheetID, sheetID, datas[0], datas), OnError, onWriteCallback);

#endif
        } 
          

 


#endregion

#region OdinInsepctorExtentions
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Button("UploadToSheet")]
    public void Upload()
    {
        Write(this);
    }
 
    
#endif


 
#endregion
    public static void OnError(System.Exception e){
         UnityGoogleSheet.OnTableError(e);
    }
 
    }
}
        