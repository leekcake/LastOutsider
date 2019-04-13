using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Switch animation by enable/disable game project
/// </summary>
public class AnimationSwitcher : MonoBehaviour {
    public GameObject nextAnimation;

    public void Switch()
    {
        gameObject.SetActive(false);
        nextAnimation.SetActive(true);
    }
}
