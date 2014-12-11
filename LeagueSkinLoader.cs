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

      private readonly CharacterInibinKeyHashes[] sknHashLookup = {
         CharacterInibinKeyHashes.SkinBaseSkn,
         CharacterInibinKeyHashes.SkinOneSkn,
         CharacterInibinKeyHashes.SkinTwoSkn,
         CharacterInibinKeyHashes.SkinThreeSkn,
         CharacterInibinKeyHashes.SkinFourSkn,
         CharacterInibinKeyHashes.SkinFiveSkn,
         CharacterInibinKeyHashes.SkinSixSkn,
         CharacterInibinKeyHashes.SkinSevenSkn
      };
      private readonly CharacterInibinKeyHashes[] sklHashLookup = {
         CharacterInibinKeyHashes.SkinBaseSkl,
         CharacterInibinKeyHashes.SkinOneSkl,
         CharacterInibinKeyHashes.SkinTwoSkl,
         CharacterInibinKeyHashes.SkinThreeSkl,
         CharacterInibinKeyHashes.SkinFourSkl,
         CharacterInibinKeyHashes.SkinFiveSkl,
         CharacterInibinKeyHashes.SkinSixSkl,
         CharacterInibinKeyHashes.SkinSevenSkl
      };
      private readonly CharacterInibinKeyHashes[] textureHashLookup = {
         CharacterInibinKeyHashes.SkinBaseTexture,
         CharacterInibinKeyHashes.SkinOneTexture,
         CharacterInibinKeyHashes.SkinTwoTexture,
         CharacterInibinKeyHashes.SkinThreeTexture,
         CharacterInibinKeyHashes.SkinFourTexture,
         CharacterInibinKeyHashes.SkinFiveTexture,
         CharacterInibinKeyHashes.SkinSixTexture,
         CharacterInibinKeyHashes.SkinSevenTexture
      };


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
            if (skinNumber > 7) {
               // TODO: Find out if there are any old champions that have 8 skins or more. So I can get the hash codes.
               return null;
            }

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

               var skin = new LoLSkin {
                  sknFilePath = "DATA/Characters/" + championName + "/" + (string)inibinFile.Properties[(uint)sknHashLookup[skinNumber]],
                  sklFilePath = "DATA/Characters/" + championName + "/" + (string)inibinFile.Properties[(uint)sklHashLookup[skinNumber]],
                  textureFilePath = "DATA/Characters/" + championName + "/" + (string)inibinFile.Properties[(uint)textureHashLookup[skinNumber]]
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
