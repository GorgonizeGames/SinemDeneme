using UnityEngine;

namespace Game.Runtime.UI.Core
{
    public enum UILayer
    {
        Background = 0,
        Game = 100,
        HUD = 200,
        Menu = 300,
        Popup = 400,
        System = 500,
        Debug = 1000
    }

    public enum UITransition
    {
        None,
        Fade,
        Scale,
        Slide,
        FadeScale
    }
}