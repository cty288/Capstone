using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Polyglot;
using UnityEngine;

namespace Runtime.Utilities.ConfigSheet {
	public class ConfigTable {
		private string docID;
		private string sheetID;
		private string localBackupName;
		
		public ConfigTable(string docID, string sheetID, string localBackupName) {
			this.docID = docID;
			this.sheetID = sheetID;
			this.localBackupName = localBackupName;
			Init();
		}

		private void Init() {
			string result = "";
			var enumerator = LoadOrDownload(docID, sheetID, localBackupName, s => {
				result = s;
			});
			while (enumerator.MoveNext()) {
				
			}
			OnLoadFinished(result);
		}


		private Dictionary<string, Dictionary<string, dynamic>> data =
			new Dictionary<string, Dictionary<string, dynamic>>();
		private void OnLoadFinished(string text) {
			List<List<string>> rows;
            text = text.Replace("\r\n", "\n");
            rows = CsvReader.Parse(text);
            var canBegin = false;
            
            string[] headers = new string[rows[0].Count-1];
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
                List<string> row = rows[rowIndex];
                if (rowIndex == 0) {
	                //load the header
	                for (int i = 1; i < row.Count; i++) {
		                headers[i-1] = row[i];
	                }
	                continue;
                }
                
                
                string name = row[0];
                
                if (string.IsNullOrEmpty(name) || IsLineBreak(name) || row.Count <= 1) {
                    //Ignore empty lines in the sheet
                    continue;
                }
                
                if (!canBegin)
                {
                    if (name == "Polyglot" || name == "PolyMaster" || name == "BEGIN") {
                        canBegin = true;
                    }
                    continue;
                }

                if (name == "END") {
                    break;
                }

                //Remove key
                row.RemoveAt(0);
                Dictionary<string, dynamic> rowDict = new Dictionary<string, dynamic>();
                for (int i = 0; i < row.Count; i++) {
	                dynamic value = JsonConvert.DeserializeObject<dynamic>(row[i]);
	                if(value is null || value.Equals("")) {
		                continue;
	                }
	                if (value is double) {
		                value = (float) value;
	                }else if (value is long) {
		                value = (int) value;
	                }
	                rowDict.Add(headers[i], value);
                }

                data.Add(name, rowDict);

            }
		}
		
		public dynamic Get(string entityName, string key) {
			if (data.ContainsKey(entityName)) {
				if (data[entityName].ContainsKey(key)) {
					return data[entityName][key];
				}
			}

			return null;
		}
		
		private static bool IsLineBreak(string currentString) {
			return currentString.Length == 1 && (currentString[0] == '\r' || currentString[0] == '\n')
			       || currentString.Length == 2 && currentString.Equals(Environment.NewLine);
		}

		private static IEnumerator LoadOrDownload(string docID, string sheetID, string localBackupName, Action<string> onDone) {
			TextAsset asset = Resources.Load<TextAsset>(localBackupName);
			
			//check if we have access to the internet and the url
			if (Application.internetReachability == NetworkReachability.NotReachable && asset) {
				Debug.Log("No internet connection, loading local backup");
				onDone(asset.text);
				yield break;
			}

			string result = "";
			var enumerator = GoogleDownload.DownloadSheet(docID,
				sheetID, s => {
					result = s;
				}, GoogleDriveDownloadFormat.CSV);
			while (enumerator.MoveNext()) {
				
			}
			
			if (string.IsNullOrEmpty(result) && asset) {
				Debug.Log("No internet connection, loading local backup");
				onDone(asset.text);
				yield break;
			}

			Debug.Log("Sheet loaded from Google Successfully! Sheet ID: " + sheetID);
			onDone(result);
			//ping the url to see if it exists
			if (Application.isEditor) {
				//save the file to Resources
				File.WriteAllText(Application.dataPath + "/Resources/" + localBackupName + ".csv", result);
				Debug.Log("Saved to Resources");
			}
			
		}
	}
}