using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISerialization
{
    public void StageForSerialization();

    public void RestoreAfterDeserialization(GameManager gameManager);
}
