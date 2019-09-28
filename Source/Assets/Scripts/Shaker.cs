using UnityEngine;
using System.Collections;

public class Shaker
{


    public GameObject container;
    public Rock rock;

    public void shakeBox(bool isShaking, bool isShakingR, float shakingFraction, float shakingRFraction)
    {
        if (isShaking == true)
        {
            Vector3 currentPosition = container.GetComponent<Transform>().position;
            Vector3 boxScale = container.GetComponent<Transform>().localScale;
            Vector3 shake = new Vector3(UnityEngine.Random.Range(-1f, 1f) * shakingFraction * boxScale.x, UnityEngine.Random.Range(-1f, 1f) * shakingFraction * boxScale.y, UnityEngine.Random.Range(-1f, 1f) * shakingFraction * boxScale.z);
            container.GetComponent<Rigidbody>().MovePosition(currentPosition + shake);
            rock.MoveCementedGrains(shake, Quaternion.identity);
            rock.WakeUp();

        }

        if (isShakingR == true)
        {
            Quaternion shakeR = Quaternion.Euler(180 * UnityEngine.Random.Range(-1f, 1f) * shakingRFraction, 180 * UnityEngine.Random.Range(-1f, 1f) * shakingRFraction, 180 * UnityEngine.Random.Range(-1f, 1f) * shakingRFraction);
            container.GetComponent<Rigidbody>().MoveRotation(shakeR);
            rock.MoveCementedGrains(new Vector3(), shakeR);
            rock.WakeUp();
        }

    }





}
