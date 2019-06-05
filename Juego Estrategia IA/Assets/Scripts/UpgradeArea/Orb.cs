using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    private SpriteRenderer typeMarker;

    private void Start()
    {
        typeMarker = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetOrbType(Sprite orbType)
    {
        if(typeMarker == null)
            typeMarker = GetComponentInChildren<SpriteRenderer>();

        typeMarker.sprite = orbType;
    }
}
