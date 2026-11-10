using System;
using System.IO;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace Battlefield.Editor.PlayerCharacter
{
    public static class MixamoArmedLocomotionImporter
    {
        public const string Folder = "Assets/ThirdParty/MixamoLocal/ArmedLocomotion";
        public const string ProfilePath = Folder + "/ArmedLocomotion.asset";
        public const string RifleController = Folder + "/RifleLocomotion.controller";
        public const string PistolController = Folder + "/PistolLocomotion.controller";
        const string BaseController = "Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller";
        static readonly string[] Rifle = {"rifle aiming idle","walking","walking backwards","strafe left","strafe right","rifle run","run backwards","strafe (2)","strafe"};
        static readonly string[] Pistol = {"pistol idle","pistol walk","pistol walk backward","pistol strafe","pistol strafe (2)","pistol run","pistol run backward"};
        static readonly Vector2[] Positions = {Vector2.up,Vector2.down,Vector2.left,Vector2.right};
        [MenuItem("Tools/Battlefield/Animations/Mixamo/Import Armed Locomotion")]
        public static void Import()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../LocalAssets/Mixamo"));
            string[] relative=Rifle.Select(n=>"Basic Shooter Pack/"+n+".fbx").Concat(Pistol.Select(n=>"Pistol_Handgun Locomotion Pack/"+n+".fbx")).ToArray();
            foreach(string file in relative) if(!File.Exists(Path.Combine(root,file))) throw new FileNotFoundException("Required external original is missing; import stopped.",Path.Combine(root,file));
            foreach(string file in relative)
            {
                string path=Folder+"/Sources/"+file;EnsureFolder(Path.GetDirectoryName(path).Replace('\\','/'));
                bool first=!File.Exists(path);byte[] original=File.ReadAllBytes(Path.Combine(root,file));
                if(first||!File.ReadAllBytes(path).SequenceEqual(original))File.WriteAllBytes(path,original);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                // A second import may refresh source bytes, but keeps authored settings.
                if(!first&&importer.clipAnimations.Length>0)continue;
                importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importCameras=importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
                importer.animationCompression=ModelImporterAnimationCompression.Off;
                var take=importer.defaultClipAnimations.First();
                importer.clipAnimations=new[]{new ModelImporterClipAnimation{name=Path.GetFileNameWithoutExtension(file),takeName=take.takeName,
                    firstFrame=take.firstFrame,lastFrame=take.lastFrame,loopTime=true,loopPose=false,
                    lockRootRotation=true,keepOriginalOrientation=false,lockRootHeightY=true,keepOriginalPositionY=true,
                    lockRootPositionXZ=false,keepOriginalPositionXZ=false}};importer.SaveAndReimport();
            }
            Build(RifleController,"Basic Shooter Pack",Rifle,false);Build(PistolController,"Pistol_Handgun Locomotion Pack",Pistol,true);
            var profile=AssetDatabase.LoadAssetAtPath<ArmedLocomotionAnimationSet>(ProfilePath);
            if(profile==null)
            {
                profile=ScriptableObject.CreateInstance<ArmedLocomotionAnimationSet>();AssetDatabase.CreateAsset(profile,ProfilePath);
                var data=new SerializedObject(profile);var entries=data.FindProperty("_entries");entries.arraySize=2;
                SetEntry(entries,0,BaseController,PistolController);
                SetEntry(entries,1,"Assets/Features/Weapon/Definitions/AssaultRifle.overrideController",RifleController);
                data.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(profile);
            }
            Debug.Log("MIXAMO_ARMED_LOCOMOTION_READY "+ProfilePath+"; existing settings and profiles preserved.");
        }
        static void SetEntry(SerializedProperty entries,int i,string equipment,string movement)
        {
            var entry=entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("_equipment").objectReferenceValue=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(equipment);
            entry.FindPropertyRelative("_movement").objectReferenceValue=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(movement);
        }
        static void Build(string path,string source,string[] names,bool pistol)
        {
            if(AssetDatabase.LoadAssetAtPath<AnimatorController>(path)!=null)return;
            if(!AssetDatabase.CopyAsset(BaseController,path))throw new InvalidOperationException("Could not create local movement controller.");
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            AnimationClip Clip(string name)
            {
                var assets=AssetDatabase.LoadAllAssetsAtPath(Folder+"/Sources/"+source+"/"+name+".fbx");
                var avatar=assets.OfType<Avatar>().FirstOrDefault();var clip=assets.OfType<AnimationClip>().Single(c=>!c.name.StartsWith("__"));
                if(!clip.humanMotion||avatar==null||!avatar.isHuman||!avatar.isValid)throw new InvalidOperationException("Invalid Humanoid source: "+name);return clip;
            }
            BlendTree Direction(string name,string[] clips,bool slowSides)
            {
                var tree=new BlendTree{name=name,blendType=BlendTreeType.FreeformDirectional2D,blendParameter="MoveX",blendParameterY="MoveY",useAutomaticThresholds=false};
                AssetDatabase.AddObjectToAsset(tree,controller);
                tree.children=clips.Select((clip,i)=>new ChildMotion{motion=Clip(clip),position=Positions[i],timeScale=slowSides&&i>=2?.5f:1f}).ToArray();return tree;
            }
            var walk=Direction("Armed walk",new[]{names[1],names[2],names[3],names[4]},pistol);
            var run=Direction("Armed run",pistol?new[]{names[5],names[6],names[3],names[4]}:new[]{names[5],names[6],names[7],names[8]},false);
            var speed=new BlendTree{name="Armed locomotion",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(speed,controller);
            speed.children=new[]{new ChildMotion{motion=Clip(names[0]),threshold=0,timeScale=1},new ChildMotion{motion=walk,threshold=.5f,timeScale=1},
                new ChildMotion{motion=run,threshold=1,timeScale=1},new ChildMotion{motion=run,threshold=2,timeScale=1.5f}};
            controller.layers[0].stateMachine.states.Single(s=>s.state.name=="Locomotion").state.motion=speed;
            EditorUtility.SetDirty(controller);AssetDatabase.SaveAssetIfDirty(controller);
        }
        static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return;string parent=path.Substring(0,path.LastIndexOf('/'));EnsureFolder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
        }
    }
}
