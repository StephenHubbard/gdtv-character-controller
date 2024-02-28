using UnityEditor;
using UnityEngine;
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

public static class Setup 
{
   [MenuItem("Tools/Setup/Create Default Folder Structure", priority = 0)]
   public static void CreateDefaultFolder()
   {
      Folders.CreateDefault("_Project", "Animation", "Art", "Materials", "Prefabs", "ScriptableObjects", "Scripts", "Settings", "Scenes");
      Refresh();
   }

   [MenuItem("Tools/Setup/Import Favorite Assets", priority = 1)]
   public static void ImportFavoriteAssets()
   {
      Assets.ImportAsset("DOTween Pro.unitypackage", "Demigiant/Editor ExtensionsVisual Scripting");
   }

   static class Folders
   {
      public static void CreateDefault(string root, params string[] folders)
      {
         var fullPath = Combine(Application.dataPath, root);
         foreach (var folder in folders)
         {
            var path = Combine(fullPath, folder);
            if (!Exists(path))
            {
               CreateDirectory(path);
            }
         }
      }
   }

   public static class Assets {
      public static void ImportAsset(string asset, string subfolder, string rootFolder = "C:\\Users\\Owner\\AppData\\Roaming\\Unity\\Asset Store-5.x") {
         ImportPackage(Combine(rootFolder, subfolder, asset), false);
      }
   }
}

