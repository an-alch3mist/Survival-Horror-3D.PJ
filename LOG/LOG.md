# LOG.md created, perform LOG.SaveLog(str, format) to append text here:

```cutSelection

# well lets see how does that go
def _A():
    print("somthng")
    list = [0.1, 1/(2**6)]
    for i in range(100):
        print(i)
        # walk(1, 0)
        sleep(0)
        list.append(i)
    list.append(2*(5**2*2))
    list.sort(key = lambda val: len(str(val)))
    return (2**14, list)
    
    
    
print(_A())
        
        
```

```sceneGameObject-hierarchy
=== Component Abbreviations ===
asrc = AudioSource
alstn = AudioListener
cam = Camera
================================

./player/(scale:1.0 | asrc, CharacterController, FirstPersonController, FirstPersonGrabber)
└ playerCam (scale:1.0 | alstn, cam, UniversalAdditionalCameraData)

```

```sceneGameObject-hierarchy
=== Component Abbreviations ===
dmc = MeshFilter | MeshRenderer
mc = MeshCollider
================================

./proBuilder/(scale:1.0 | no components)
├ Plane (scale:1.0 | dmc, mc, ProBuilderMesh, ProBuilderShape)
├ Stairs (scale:1.0 | dmc, mc, ProBuilderMesh, ProBuilderShape)
├ Arch (scale:1.0 | dmc, mc, ProBuilderMesh, ProBuilderShape)
├ Cylinder (scale:1.0 | dmc, mc, ProBuilderMesh, ProBuilderShape)
├ Cube (scale:1.0 | dmc, mc, ProBuilderMesh, ProBuilderShape)
├ Cube (1) (scale:1.0 | dmc, mc, ProBuilderMesh, ProBuilderShape)
└ Cube (2) (scale:1.0 | dmc, mc, ProBuilderMesh, ProBuilderShape)

```

```projectFolder-hierarchy
=== Asset Type Abbreviations ===
tex = Texture
cs = Script
scene = Scene
================================

./URP-BuiltIn/
├ InputSystem_Actions.inputactions (InputActionAsset)
│ ├ Player/Attack (InputActionReference)
│ ├ Player/Crouch (InputActionReference)
│ ├ Player/Interact (InputActionReference)
│ ├ Player/Jump (InputActionReference)
│ ├ Player/Look (InputActionReference)
│ ├ Player/Move (InputActionReference)
│ ├ Player/Next (InputActionReference)
│ ├ Player/Previous (InputActionReference)
│ ├ Player/Sprint (InputActionReference)
│ ├ UI/Cancel (InputActionReference)
│ ├ UI/Click (InputActionReference)
│ ├ UI/MiddleClick (InputActionReference)
│ ├ UI/Navigate (InputActionReference)
│ ├ UI/Point (InputActionReference)
│ ├ UI/RightClick (InputActionReference)
│ ├ UI/ScrollWheel (InputActionReference)
│ ├ UI/Submit (InputActionReference)
│ ├ UI/TrackedDeviceOrientation (InputActionReference)
│ └ UI/TrackedDevicePosition (InputActionReference)
├ Readme.asset (Readme)
├ Scenes/
│ └ SampleScene.unity (scene)
├ Settings/
│ ├ DefaultVolumeProfile.asset (VolumeProfile)
│ ├ Mobile_Renderer.asset (UniversalRendererData)
│ ├ Mobile_RPAsset.asset (UniversalRenderPipelineAsset)
│ ├ PC_Renderer.asset (UniversalRendererData)
│ │ ├ OutlineRendererFeature (OutlineRendererFeature)
│ │ └ ScreenSpaceAmbientOcclusion (ScreenSpaceAmbientOcclusion)
│ ├ PC_RPAsset.asset (UniversalRenderPipelineAsset)
│ ├ SampleSceneProfile.asset (VolumeProfile)
│ └ UniversalRenderPipelineGlobalSettings.asset (UniversalRenderPipelineGlobalSettings)
└ TutorialInfo/
  ├ Icons/
  │ └ URP.png (tex | 350×200 | RGB24)
  ├ Layout.wlt (DefaultAsset)
  └ Scripts/
    ├ Editor/
    │ └ ReadmeEditor.cs (cs | ReadmeEditor)
    └ Readme.cs (cs | Readme)

```

```projectFolder-hierarchy
./Settings/
├ DefaultVolumeProfile.asset (VolumeProfile)
├ Mobile_Renderer.asset (UniversalRendererData)
├ Mobile_RPAsset.asset (UniversalRenderPipelineAsset)
├ PC_Renderer.asset (UniversalRendererData)
│ ├ OutlineRendererFeature (OutlineRendererFeature)
│ └ ScreenSpaceAmbientOcclusion (ScreenSpaceAmbientOcclusion)
├ PC_RPAsset.asset (UniversalRenderPipelineAsset)
├ SampleSceneProfile.asset (VolumeProfile)
└ UniversalRenderPipelineGlobalSettings.asset (UniversalRenderPipelineGlobalSettings)

```

```sceneGameObject-hierarchy
=== Component Abbreviations ===
dmc = MeshFilter | MeshRenderer
bc = BoxCollider
================================

./Cube (1) with gradient/(scale:1.0 | dmc, bc, HighlightEffect, HighlightTrigger)

```

