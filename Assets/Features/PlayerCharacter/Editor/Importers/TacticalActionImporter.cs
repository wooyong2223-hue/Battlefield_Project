using System;
using System.IO;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Projectile;
using Battlefield.Features.Weapon;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace Battlefield.Editor.PlayerCharacter
{
    public static class TacticalActionImporter
    {
        public const string Folder = "Assets/ThirdParty/MixamoLocal/TacticalActions";
        public const string ProfilePath = Folder + "/TacticalActions.asset";
        const string Placeholders = "Assets/Features/PlayerCharacter/Animation/Tactical";
        const string Base = "Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller";
        [MenuItem("Tools/Battlefield/Animations/Mixamo/Import Grenade And Mantle")]
        public static void Import()
        {
            Ensure(Folder); Ensure(Placeholders);
            AnimationClip ImportClip(string name)
            {
                string source=Path.GetFullPath(Path.Combine(Application.dataPath,"../LocalAssets/Mixamo/"+name+".fbx"));
                if(!File.Exists(source))throw new FileNotFoundException("Required external animation missing",source);
                string path=Folder+"/"+name+".fbx";bool first=!File.Exists(path);
                if(first)File.Copy(source,path);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                if(first || importer.clipAnimations.Length==0)
                {
                    importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.importCameras=importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
                    importer.animationCompression=ModelImporterAnimationCompression.Off;
                    var take=importer.defaultClipAnimations.First();
                    // Extract source travel: the motor is the only world-position authority.
                    importer.clipAnimations=new[]{new ModelImporterClipAnimation{name=name,takeName=take.takeName,firstFrame=take.firstFrame,lastFrame=take.lastFrame,
                        loopTime=false,loopPose=false,lockRootRotation=true,keepOriginalOrientation=true,
                        lockRootHeightY=false,keepOriginalPositionY=false,heightFromFeet=true,lockRootPositionXZ=false,keepOriginalPositionXZ=true}};
                    importer.SaveAndReimport();
                }
                var all=AssetDatabase.LoadAllAssetsAtPath(path);var avatar=all.OfType<Avatar>().First();var clip=all.OfType<AnimationClip>().First(c=>!c.name.StartsWith("__"));
                if(!avatar.isValid || !avatar.isHuman || !clip.humanMotion)throw new InvalidOperationException("Invalid Humanoid "+name);
                return clip;
            }
            var mantle=ImportClip("Braced Hang To Crouch");var throwing=ImportClip("Throw Grenade");
            AnimationClip Placeholder(string name,string source)
            {
                string path=Placeholders+"/"+name+".anim";
                if(!File.Exists(path))AssetDatabase.CopyAsset(source,path);
                return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            }
            var mantleKey=Placeholder("MantlePlaceholder","Assets/ThirdParty/Quaternius/Animations/ClimbUp_1m.anim");
            var throwKey=Placeholder("ThrowPlaceholder","Assets/ThirdParty/Quaternius/Library/UAL2/InPlace/OverhandThrow.anim");
            string prefabPath="Assets/Features/Projectile/Prefabs/FragGrenade.prefab";
            if(!File.Exists(prefabPath))
            {
                var root=new GameObject("FragGrenade");var projectile=root.AddComponent<GrenadeProjectile>();
                root.GetComponent<Rigidbody>().isKinematic=true;root.GetComponent<SphereCollider>().radius=.12f;
                var visual=GameObject.CreatePrimitive(PrimitiveType.Capsule);visual.name="Visual";UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
                visual.transform.SetParent(root.transform,false);visual.transform.localScale=new Vector3(.2f,.14f,.2f);
                string materialPath="Assets/Features/Projectile/Prefabs/FragGrenade.mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard"));material.color=new Color(.22f,.28f,.1f);AssetDatabase.CreateAsset(material,materialPath);}
                visual.GetComponent<Renderer>().sharedMaterial=material;PrefabUtility.SaveAsPrefabAsset(root,prefabPath);UnityEngine.Object.DestroyImmediate(root);
            }
            string bouncePath="Assets/Features/Projectile/Prefabs/FragGrenade.physicMaterial";
            var bounce=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(bouncePath);
            if(bounce==null){bounce=new PhysicsMaterial("FragGrenade"){bounciness=.35f,dynamicFriction=.4f,staticFriction=.5f};AssetDatabase.CreateAsset(bounce,bouncePath);}
            var prefabRoot=PrefabUtility.LoadPrefabContents(prefabPath);
            try{var sphere=prefabRoot.GetComponent<SphereCollider>();if(sphere.sharedMaterial==null){sphere.sharedMaterial=bounce;PrefabUtility.SaveAsPrefabAsset(prefabRoot,prefabPath);}}
            finally{PrefabUtility.UnloadPrefabContents(prefabRoot);}
            var grenade=AssetDatabase.LoadAssetAtPath<GrenadeDefinition>(Folder+"/FragGrenade.asset");
            if(grenade==null)
            {
                grenade=ScriptableObject.CreateInstance<GrenadeDefinition>();AssetDatabase.CreateAsset(grenade,Folder+"/FragGrenade.asset");
                var data=new SerializedObject(grenade);data.FindProperty("_projectile").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GrenadeProjectile>(prefabPath);
                data.FindProperty("_throwAnimation").objectReferenceValue=throwing;data.ApplyModifiedPropertiesWithoutUndo();
            }
            var definition=AssetDatabase.LoadAssetAtPath<MantleDefinition>(Folder+"/Mantle.asset");
            if(definition==null){definition=ScriptableObject.CreateInstance<MantleDefinition>();AssetDatabase.CreateAsset(definition,Folder+"/Mantle.asset");var data=new SerializedObject(definition);data.FindProperty("_animation").objectReferenceValue=mantle;data.ApplyModifiedPropertiesWithoutUndo();}
            var profile=AssetDatabase.LoadAssetAtPath<TacticalAnimationSet>(ProfilePath);
            if(profile==null)
            {
                profile=ScriptableObject.CreateInstance<TacticalAnimationSet>();AssetDatabase.CreateAsset(profile,ProfilePath);var data=new SerializedObject(profile);
                data.FindProperty("_mantlePlaceholder").objectReferenceValue=mantleKey;data.FindProperty("_throwPlaceholder").objectReferenceValue=throwKey;
                data.FindProperty("_mantle").objectReferenceValue=definition;data.FindProperty("_grenade").objectReferenceValue=grenade;data.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach(string path in new[]{Base,MixamoArmedLocomotionImporter.RifleController,MixamoArmedLocomotionImporter.PistolController})
            {
                var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);if(controller==null)continue;
                void Parameter(string name){if(!controller.parameters.Any(p=>p.name==name))controller.AddParameter(name,AnimatorControllerParameterType.Float);}
                Parameter("MantleTime");Parameter("ThrowTime");
                var machine=controller.layers[0].stateMachine;
                if(!machine.states.Any(s=>s.state.name=="Mantle"))
                {
                    var state=machine.AddState("Mantle");state.motion=mantleKey;state.timeParameterActive=true;state.timeParameter="MantleTime";
                }
                if(!controller.layers.Any(l=>l.name=="GrenadeUpperBody"))
                {
                    var upper=controller.layers.First(l=>l.name=="UpperCombat");
                    var layer=new AnimatorControllerLayer{name="GrenadeUpperBody",defaultWeight=0f,blendingMode=AnimatorLayerBlendingMode.Override,avatarMask=upper.avatarMask,
                        stateMachine=new AnimatorStateMachine{name="GrenadeUpperBody",hideFlags=HideFlags.HideInHierarchy}};
                    AssetDatabase.AddObjectToAsset(layer.stateMachine,controller);var state=layer.stateMachine.AddState("Throw");state.motion=throwKey;state.timeParameterActive=true;state.timeParameter="ThrowTime";layer.stateMachine.defaultState=state;
                    controller.AddLayer(layer);
                }
                EditorUtility.SetDirty(controller);AssetDatabase.SaveAssetIfDirty(controller);
            }
            GrenadeHandlingAnimationAuthoring.Ensure();
            AssetDatabase.SaveAssets();Debug.Log("TACTICAL_IMPORT_OK");
        }
        static void Ensure(string folder)
        {if(AssetDatabase.IsValidFolder(folder))return;Ensure(Path.GetDirectoryName(folder).Replace('\\','/'));AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\','/'),Path.GetFileName(folder));}
    }
}
