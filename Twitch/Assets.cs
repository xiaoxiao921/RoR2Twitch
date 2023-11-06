using System.IO;
using R2API;
using UnityEngine;

namespace Twitch
{
    public static class Assets
    {
        public static void PopulateAssets()
        {
            var assetsFolder = Path.GetDirectoryName(Twitch.Instance.Info.Location);
            Assets.MainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(assetsFolder, "twitch"));
            SoundAPI.SoundBanks.Add(Path.Combine(assetsFolder, "Twitch.bnk"));

            Assets.charPortrait = Assets.MainAssetBundle.LoadAsset<Sprite>("TwitchBody").texture;
            Assets.iconP = Assets.MainAssetBundle.LoadAsset<Sprite>("VenomIcon");
            Assets.icon1 = Assets.MainAssetBundle.LoadAsset<Sprite>("SprayAndPrayIcon");
            Assets.icon1b = Assets.MainAssetBundle.LoadAsset<Sprite>("TommyGunIcon");
            Assets.icon1c = Assets.MainAssetBundle.LoadAsset<Sprite>("ShotgunIcon");
            Assets.icon1d = Assets.MainAssetBundle.LoadAsset<Sprite>("BazookaIcon");
            Assets.icon2 = Assets.MainAssetBundle.LoadAsset<Sprite>("CaskIcon");
            Assets.icon2b = Assets.MainAssetBundle.LoadAsset<Sprite>("GrenadeIcon");
            Assets.icon3 = Assets.MainAssetBundle.LoadAsset<Sprite>("AmbushIcon");
            Assets.icon3b = Assets.MainAssetBundle.LoadAsset<Sprite>("AmbushActiveIcon");
            Assets.icon3c = Assets.MainAssetBundle.LoadAsset<Sprite>("AmbushRecastIcon");
            Assets.icon3d = Assets.MainAssetBundle.LoadAsset<Sprite>("CheeseIcon");
            Assets.icon4 = Assets.MainAssetBundle.LoadAsset<Sprite>("ExpungeIcon");
            Assets.arrowModel = Assets.MainAssetBundle.LoadAsset<GameObject>("ArrowModel");
            Assets.caskModel = Assets.MainAssetBundle.LoadAsset<GameObject>("CaskModel");
            Assets.bazookaRocketModel = Assets.MainAssetBundle.LoadAsset<GameObject>("BazookaRocketModel");
            Assets.grenadeModel = Assets.MainAssetBundle.LoadAsset<GameObject>("GrenadeModel");
            Assets.knifeModel = Assets.MainAssetBundle.LoadAsset<GameObject>("KnifeModel");
            Assets.mainSkinMat = Assets.MainAssetBundle.LoadAsset<Material>("matTwitch");
            Assets.simpleSkinMat = Assets.MainAssetBundle.LoadAsset<Material>("matTwitchSimple");
            Assets.tarSkinMat = Assets.MainAssetBundle.LoadAsset<Material>("matTwitchTar");
            Assets.tundraSkinMat = Assets.MainAssetBundle.LoadAsset<Material>("matTwitchTundra");
        }

        // Token: 0x0400009E RID: 158
        public static AssetBundle MainAssetBundle = null;

        // Token: 0x0400009F RID: 159
        public static Texture charPortrait;

        // Token: 0x040000A0 RID: 160
        public static Sprite iconP;

        // Token: 0x040000A1 RID: 161
        public static Sprite icon1;

        // Token: 0x040000A2 RID: 162
        public static Sprite icon1b;

        // Token: 0x040000A3 RID: 163
        public static Sprite icon1c;

        // Token: 0x040000A4 RID: 164
        public static Sprite icon1d;

        // Token: 0x040000A5 RID: 165
        public static Sprite icon2;

        // Token: 0x040000A6 RID: 166
        public static Sprite icon2b;

        // Token: 0x040000A7 RID: 167
        public static Sprite icon3;

        // Token: 0x040000A8 RID: 168
        public static Sprite icon3b;

        // Token: 0x040000A9 RID: 169
        public static Sprite icon3c;

        // Token: 0x040000AA RID: 170
        public static Sprite icon3d;

        // Token: 0x040000AB RID: 171
        public static Sprite icon4;

        // Token: 0x040000AC RID: 172
        public static GameObject arrowModel;

        // Token: 0x040000AD RID: 173
        public static GameObject caskModel;

        // Token: 0x040000AE RID: 174
        public static GameObject bazookaRocketModel;

        // Token: 0x040000AF RID: 175
        public static GameObject grenadeModel;

        // Token: 0x040000B0 RID: 176
        public static GameObject knifeModel;

        // Token: 0x040000B1 RID: 177
        public static Material mainSkinMat;

        // Token: 0x040000B2 RID: 178
        public static Material simpleSkinMat;

        // Token: 0x040000B3 RID: 179
        public static Material tarSkinMat;

        // Token: 0x040000B4 RID: 180
        public static Material tundraSkinMat;
    }
}
