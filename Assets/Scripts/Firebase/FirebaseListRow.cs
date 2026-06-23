using TMPro;
using UnityEngine;

 
public class FirebaseListRow : MonoBehaviour
{
    [SerializeField]
    private TMP_Text col1;

    [SerializeField]
    private TMP_Text col2;

    [SerializeField]
    private TMP_Text col3;

    public void Set(string a, string b, string c)
    {
        if (col1 != null) col1.text = a;
        if (col2 != null) col2.text = b;
        if (col3 != null) col3.text = c;
    }
}
