using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRenderVisible : MonoBehaviour
{
    private bool openAABB = false;
    private Collider2D box;
    private GameObject firstChild;

    void Start()
    {
        this.box = this.GetComponent<Collider2D>();

        if (this.firstChild == null)
        {
            this.firstChild = transform.Find("RenderNode").gameObject;
        }
        openAABB = true;
        StartCoroutine(Check());
    }

    IEnumerator Check()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        while (openAABB)
        {
            if (this == null || firstChild == null)
            {
                yield break;
            }
            else
            {
                if (GeometryUtility.TestPlanesAABB(UIRoot.Instance.CameraPlanes, this.box.bounds))
                {
                    this.firstChild.SetActive(true);
                }
                else
                {
                    this.firstChild.SetActive(false);
                }

                yield return 2;
            }
        }
        yield break;
    }

    public void ForceActive()
    {
        openAABB = false;
        this.firstChild.SetActive(true);
        StartCoroutine(ResumeCheck());
    }

    IEnumerator ResumeCheck()
    {
        yield return new WaitForSeconds(5f);
        openAABB = true;
        StartCoroutine(Check());
    }
}
