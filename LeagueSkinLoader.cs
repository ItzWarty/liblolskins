using System;
using System.IO;
using Dargon.FileSystem;

namespace Dargon.LeagueOfLegends.Skins {
   public class LoLSkin {
      public string sknFilePath;
      public string sklFilePath;
      public string textureFilePath;
      public string loadScreenTextureFilePath;
   }

   public class LeagueSkinLoader {
      private readonly IInibinLoader inibinLoader;

      public LeagueSkinLoader() {
         inibinLoader = new InibinLoader();
      }

      public LoLSkin GetSkinForChampion(IFileSystem system, string championFolderName, uint skinNumber) {
         var root = system.AllocateRootHandle();

         IFileSystemHandle charactersFolder;
         if (system.AllocateRelativeHandleFromPath(root, @"DATA/Characters", out charactersFolder) != IoResult.Success) {
            // TODO: Real error handling. Figure out the best way to return data / errors
            return null;
         }

         IFileSystemHandle[] champions;
         if (system.AllocateChildrenHandles(charactersFolder, out champions) != IoResult.Success) {
            return null;
         }

         foreach (var champion in champions) {
            string name;
            if (system.GetName(champion, out name) != IoResult.Success) {
               return null;
            }

            if (name.Equals(championFolderName, System.StringComparison.InvariantCultureIgnoreCase)) {
               return GetSkinForChampion(system, champion, skinNumber);
            }
         }

         return null;
      }

      private CharacterInibinKeyHashes GetSknEnumForSkinNumber(uint skinNumber) {
         switch (skinNumber) {
            case 0:
               return CharacterInibinKeyHashes.SkinBaseSkn;
            case 1:
               return CharacterInibinKeyHashes.SkinOneSkn;
            case 2:
               return CharacterInibinKeyHashes.SkinTwoSkn;
            case 3:
               return CharacterInibinKeyHashes.SkinThreeSkn;
            case 4:
               return CharacterInibinKeyHashes.SkinFourSkn;
            case 5:
               return CharacterInibinKeyHashes.SkinFiveSkn;
            case 6:
               return CharacterInibinKeyHashes.SkinSixSkn;
            case 7:
               return CharacterInibinKeyHashes.SkinSevenSkn;
            default:
               throw new ArgumentOutOfRangeException("skinNumber");
         }
      }

      private CharacterInibinKeyHashes GetSklEnumForSkinNumber(uint skinNumber) {
         switch (skinNumber) {
            case 0:
               return CharacterInibinKeyHashes.SkinBaseSkl;
            case 1:
               return CharacterInibinKeyHashes.SkinOneSkl;
            case 2:
               return CharacterInibinKeyHashes.SkinTwoSkl;
            case 3:
               return CharacterInibinKeyHashes.SkinThreeSkl;
            case 4:
               return CharacterInibinKeyHashes.SkinFourSkl;
            case 5:
               return CharacterInibinKeyHashes.SkinFiveSkl;
            case 6:
               return CharacterInibinKeyHashes.SkinSixSkl;
            case 7:
               return CharacterInibinKeyHashes.SkinSevenSkl;
            default:
               throw new ArgumentOutOfRangeException("skinNumber");
         }
      }

      private CharacterInibinKeyHashes GetTextureEnumForSkinNumber(uint skinNumber) {
         switch (skinNumber) {
            case 0:
               return CharacterInibinKeyHashes.SkinBaseTexture;
            case 1:
               return CharacterInibinKeyHashes.SkinOneTexture;
            case 2:
               return CharacterInibinKeyHashes.SkinTwoTexture;
            case 3:
               return CharacterInibinKeyHashes.SkinThreeTexture;
            case 4:
               return CharacterInibinKeyHashes.SkinFourTexture;
            case 5:
               return CharacterInibinKeyHashes.SkinFiveTexture;
            case 6:
               return CharacterInibinKeyHashes.SkinSixTexture;
            case 7:
               return CharacterInibinKeyHashes.SkinSevenTexture;
            default:
               throw new ArgumentOutOfRangeException("skinNumber");
         }
      }

      private LoLSkin GetSkinForChampion(IFileSystem system, IFileSystemHandle championFolderHandle, uint skinNumber) {
         // Try to get the <championName>.inibin file
         string championName;
         if (system.GetName(championFolderHandle, out championName) != IoResult.Success) {
            return null;
         }

         IFileSystemHandle skinsFolder;
         var result = system.AllocateRelativeHandleFromPath(championFolderHandle, "Skins", out skinsFolder);
         if (!(result == IoResult.Success || result == IoResult.NotFound)) {
            return null;
         }

         if (result == IoResult.Success) {
            // New folder structure
            var folderName = skinNumber == 0 ? "Base" : "Skin" + skinNumber.ToString("D2");

            IFileSystemHandle inibinFileHandle;
            if (system.AllocateRelativeHandleFromPath(championFolderHandle, "Skins/" + folderName + "/" + folderName + ".inibin", out inibinFileHandle) != IoResult.Success) {
               return null;
            }

            byte[] inibinFileData;
            if (system.ReadAllBytes(inibinFileHandle, out inibinFileData) != IoResult.Success) {
               return null;
            }

            using (var ms = new MemoryStream(inibinFileData)) {
               var inibinFile = inibinLoader.Load(ms);

               var skin = new LoLSkin {
                  // All skins use the 'SkinBase***' hash
                  sknFilePath = "DATA/Characters/" + championName + "/Skins/ " + folderName + "/" + (string)inibinFile.Properties[(uint)CharacterInibinKeyHashes.SkinBaseSkn],
                  sklFilePath = "DATA/Characters/" + championName + "/Skins/ " + folderName + "/" + (string)inibinFile.Properties[(uint)CharacterInibinKeyHashes.SkinBaseSkl],
                  textureFilePath = "DATA/Characters/" + championName + "/Skins/ " + folderName + "/" + (string)inibinFile.Properties[(uint)CharacterInibinKeyHashes.SkinBaseTexture]
               };

               if (skinNumber == 0) {
                  skin.loadScreenTextureFilePath = "DATA/Characters/" + championName + "/Skins/ " + folderName + "/" + championName + "LoadScreen.dds";
               } else {
                  skin.loadScreenTextureFilePath = "DATA/Characters/" + championName + "/Skins/ " + folderName + "/" + championName + "LoadScreen_" + skinNumber + ".dds";
               }

               return skin;
            }
         } else {
            // Old folder structure
            IFileSystemHandle inibinFileHandle;
            if (system.AllocateRelativeHandleFromPath(championFolderHandle, championName + ".inibin", out inibinFileHandle) != IoResult.Success) {
               return null;
            }

            byte[] inibinFileData;
            if (system.ReadAllBytes(inibinFileHandle, out inibinFileData) != IoResult.Success) {
               return null;
            }

            using (var ms = new MemoryStream(inibinFileData)) {
               var inibinFile = inibinLoader.Load(ms);

               var skin = new LoLSkin
               {
                  sknFilePath = "DATA/Characters/" + championName + "/" + (string)inibinFile.Properties[(uint)GetSknEnumForSkinNumber(skinNumber)],
                  sklFilePath = "DATA/Characters/" + championName + "/" + (string)inibinFile.Properties[(uint)GetSklEnumForSkinNumber(skinNumber)],
                  textureFilePath = "DATA/Characters/" + championName + "/" + (string)inibinFile.Properties[(uint)GetTextureEnumForSkinNumber(skinNumber)]
               };

               if (skinNumber == 0) {
                  skin.loadScreenTextureFilePath = "DATA/Characters/" + championName + "/" + championName + "LoadScreen.dds";
               } else {
                  skin.loadScreenTextureFilePath = "DATA/Characters/" + championName + "/" + championName + "LoadScreen_" + skinNumber + ".dds";
               }

               return skin;
            }
         }
      }
   }
}
