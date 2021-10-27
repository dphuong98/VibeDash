using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISetup
{
    /// <summary>
    /// Load class dependencies
    /// </summary>
    void Setup();
}

public interface ICleanUp
{
    /// <summary>
    /// Prepare class for destruction
    /// </summary>
    void CleanUp();
}

public interface IBasicObject : ISetup, ICleanUp { }
