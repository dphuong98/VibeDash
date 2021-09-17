using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICopiable<T>
{
    void CopyFrom(T other);
}
