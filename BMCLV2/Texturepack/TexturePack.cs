using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;



namespace BMCLV2.Texturepack
{
    class TexturePack
    {
        DirectoryInfo ResourceDirectory;
        public TexturePackEntity[] TexturePackEntities;

        public TexturePack()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + ".minecraft\\resourcepacks"))
            {
                throw new NoResourcePackDirException();
            }
            ResourceDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + ".minecraft\\resourcepacks");
            FileInfo[] ResourcePacksInfo = ResourceDirectory.GetFiles("*.zip");
            ArrayList ResourcePackEntities = new ArrayList();
            foreach (FileInfo ResourcePack in ResourcePacksInfo)
            {
                ResourcePackEntities.Add(new TexturePackEntity(ResourcePack.FullName));
            }
            TexturePackEntities = ResourcePackEntities.ToArray(typeof(TexturePackEntity)) as TexturePackEntity[];
        }
        
    }

}
