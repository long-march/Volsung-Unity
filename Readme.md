# What is it
Use a simple synthesis language to generate or process audio in your Unity games.
<br /> Learn more about the language: https://landahl.tech/volsung

# Instructions
* Import the package
* Add the `Volsung_Unity.cs` component to a game object
* Select `Compile on Awake`
* Attach a textual Volsung program to the component
* Add an audio source to the game object

The game object will now produce sound. You can edit the audio source to set the volume or use the Unity spatialisation functionality as you would normally be able to.

If you want the game state to affect the sound processing, you can follow this general structure in an additional C# script:
```C#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    Volsung_Unity plugin;
    void Start()
    {
        plugin = GetComponent<Volsung_Unity>();
        plugin.Compile();
    }

    void Update()
    {
        // In the editor, add a parameter to the Volsung_Unity component and
        // name it "position". This will be available as a graph node in the program.
        plugin.SetParameter("position", transform.position.x);
    }
}
```

The accompanying synthesis code would be something like:
```
position -> *100 -> Smooth~ 40
-> Sine_Oscillator~ -> output
```

We are reading the position parameter, which is available as a node because it was added in the editor UI. Then we scale it, smooth it (really just a lowpass filter), and use the value as the frequency of a sine wave which is sent to the audio source component.

Unfortunately, Unity is terrible at dealing with textual assets. They cannot be created in the editor, and they must have the .txt extension. I recommend you copy-paste text assets that have already been imported and manually tell your editor to do the syntax highlighting.
