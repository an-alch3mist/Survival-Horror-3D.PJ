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

