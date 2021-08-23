using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MatrixBruteNoirControl : MonoBehaviour
{
    private Ray ray;
    private RaycastHit hit;
    private float delta;
    private float sensitivity;
    private Vector3 origin;
    private bool selected;
    private bool first;
    private UnityEngine.UI.Text HUD;
    private GameObject control;
    private Dictionary<string, Vector3> origins;
    private Dictionary<string, int> notes;

    [DllImport("__Internal")]
    private static extern void _SendMessage(int id, int msg, int x, int y);

    //static void _SendMessage(int id, int msg, int x, int y)
    //{ Debug.Log(id.ToString() + ", " + msg.ToString() + ", " + x.ToString() + ", " + y.ToString()); }

    void Start()
    {
        hit = new RaycastHit();
        ray = new Ray();
        delta = 0.0f;
        sensitivity = 0.1f;
        origin = new Vector3(0.0f, 0.0f, 0.0f);
        selected = false;
        first = true;
        HUD = GameObject.Find("/HUD/Text").GetComponent<UnityEngine.UI.Text>();
        control = null;
        origins = new Dictionary<string, Vector3>();
        notes = new Dictionary<string, int>() {{"KeyC1",24},{"KeyC#1",25},{"KeyD1",26},{"KeyD#1",27},{"KeyE1",28},{"KeyF1",29},{"KeyF#1",30},{"KeyG1",31},{"KeyG#1",32},{"KeyA1",33},{"KeyA#1",34},{"KeyB1",35},{"KeyC2",36},{"KeyC#2",37},{"KeyD2",38},{"KeyD#2",39},{"KeyE2",40},{"KeyF2",41},{"KeyF#2",42},{"KeyG2",43},{"KeyG#2",44},{"KeyA2",45},{"KeyA#2",46},{"KeyB2",47},{"KeyC3",48},{"KeyC#3",49},{"KeyD3",50},{"KeyD#3",51},{"KeyE3",52},{"KeyF3",53},{"KeyF#3",54},{"KeyG3",55},{"KeyG#3",56},{"KeyA3",57},{"KeyA#3",58},{"KeyB3",59},{"KeyC4",60},{"KeyC#4",61},{"KeyD4",62},{"KeyD#4",63},{"KeyE4",64},{"KeyF4",65},{"KeyF#4",66},{"KeyG4",67},{"KeyG#4",68},{"KeyA4",69},{"KeyA#4",70},{"KeyB4",71},{"KeyC5",72}};
        //Test();
    }

    void Update()
    {
        selected = Input.GetMouseButton(0);
        ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, out hit) && (!Input.GetMouseButton(0)))
        {
            control = hit.collider.transform.gameObject;
            HUD.text = control.name;
        }
        else if (!selected) { HUD.text = "MatrixBrute Noir"; }
        if (control != null) //Support a variety of control classes here
        {
            if (selected)
            {
                if (control.name.StartsWith("Key"))
                {
                    if (first)
                    {
                        if (!(origins.TryGetValue(control.name, out origin)))
                        {
                            origin = control.transform.eulerAngles;
                            origins.Add(control.name, origin);
                        }
                        control.transform.eulerAngles = new Vector3(origin.x, origin.y, origin.z + 2.0f);
                        int note = 0;
                        notes.TryGetValue(control.name, out note);
                        _SendMessage(1, 0x90, note, 0x7F);
                    }
                }
                if (control.name.StartsWith("Button"))
                {
                    if (first)
                    {
                        if (!(origins.TryGetValue(control.name, out origin)))
                        {
                            origin = control.transform.localPosition;
                            origins.Add(control.name, origin);
                        }
                        control.transform.localPosition = new Vector3(origin.x, origin.y + 5.0f, origin.z);
                        int cc = 0;
                        int ch = 0;
                        if (int.TryParse(control.name.Substring("Button".Length), out cc)) { cc += (64); if (cc >= (128)) { ch = (cc / (128)); cc -= (128) * (cc / (128)); } }
                        _SendMessage(1, 0xB0 + ch, cc, 0x7F);
                    }
                }
                if (control.name.StartsWith("MatrixButton"))
                {
                    if (first)
                    {
                        if (!(origins.TryGetValue(control.name, out origin)))
                        {
                            origin = control.transform.localPosition;
                            origins.Add(control.name, origin);
                        }
                        control.transform.localPosition = new Vector3(origin.x, origin.y + 5.0f, origin.z);
                        int cc = 0;
                        int ch = 0;
                        if (int.TryParse(control.name.Substring("MatrixButton".Length), out cc)) { cc -= (1); if (cc >= (128)) { ch = (cc / (128)); cc -= (128) * (cc / (128)); } }
                        _SendMessage(1, 0xBA + ch, cc, 0x7F);
                    }
                }
                if (control.name.StartsWith("Knob"))
                {
                    if (first)
                    {
                        if (!(origins.TryGetValue(control.name, out origin)))
                        {
                            origin = control.transform.localEulerAngles;
                            origins.Add(control.name, origin);
                        }
                        else { delta = control.transform.localEulerAngles.y + 90.0f; if (delta > 180.0f) { delta -= 360.0f; } } //- origin.y
                    }
                    var prev = control.transform.localEulerAngles;
                    delta += (Input.GetAxis("Mouse Y") * sensitivity * 120.0f);
                    control.transform.localEulerAngles = new Vector3(origin.x, Mathf.Clamp(origin.y + delta, origin.y - 160.0f, origin.y + 160.0f), origin.z);
                    if (control.transform.localEulerAngles != prev)
                    {
                        int cc = 0;
                        int.TryParse(control.name.Substring("Knob".Length), out cc);
                        int value = (int)(((160.0f + (control.transform.localEulerAngles.y - origin.y)) / 320.0f) * 127.0f); //Get Position
                        if (value < 0) { value += 142; }
                        _SendMessage(1, 0xB0, cc, value);
                    }
                }
                if (control.name.StartsWith("Fader"))
                {
                    if (first)
                    {
                        if (!(origins.TryGetValue(control.name, out origin)))
                        {
                            origin = control.transform.localPosition;
                            origins.Add(control.name, origin);
                        }
                        else { delta = control.transform.localPosition.x - origin.x; }
                    }
                    var prev = control.transform.localPosition;
                    delta += (Input.GetAxis("Mouse Y") * sensitivity * 100.0f);
                    control.transform.localPosition = new Vector3(Mathf.Clamp(origin.x + delta, origin.x - 100.0f, origin.x), origin.y, origin.z);
                    if (control.transform.localPosition != prev)
                    {
                        int cc = 0;
                        int.TryParse(control.name.Substring("Fader".Length), out cc);
                        int value = (int)((((0.5f + (control.transform.localPosition.x - origin.x)) / 100.0f) * 127.0f) + 127.0f); //Get Position
                        _SendMessage(1, 0xBF, cc, value);
                    }
                }
                if (control.name.StartsWith("Wheel"))
                {
                    if (first)
                    {
                        if (!(origins.TryGetValue(control.name, out origin)))
                        {
                            origin = control.transform.localEulerAngles;
                            origins.Add(control.name, origin);
                        }
                        else { delta = control.transform.localEulerAngles.x - origin.x; if (delta > 180.0f) { delta -= 360.0f; } } //- origin.y
                    }
                    var prev = control.transform.localEulerAngles;
                    delta += (Input.GetAxis("Mouse Y") * sensitivity * -60.0f);
                    control.transform.localEulerAngles = new Vector3(Mathf.Clamp(origin.x + delta, origin.x - 30.0f, origin.x + 30.0f), origin.y, origin.z);
                    if (control.transform.localEulerAngles != prev)
                    {
                        int cc = 0;
                        int.TryParse(control.name.Substring("Wheel".Length), out cc);
                        int value = (int)((((0.5f + (control.transform.localEulerAngles.x - origin.x)) / -60.0f) * 127.1f) + 64.0f); //Get Position
                        if (value <= -600) { value += 763; }
                        _SendMessage(1, 0xBE, cc, value);
                    }
                }
                first = false;
            }
            if (!selected)
            {
                if (control.name.StartsWith("Key"))
                {
                    var prev = control.transform.eulerAngles;
                    if (origins.TryGetValue(control.name, out origin)) { control.transform.eulerAngles = origin; }
                    if (control.transform.eulerAngles != prev)
                    {
                        int note = 0;
                        notes.TryGetValue(control.name, out note);
                        _SendMessage(1, 0x80, note, 0x00);
                    }
                }
                if (control.name.StartsWith("Button"))
                {
                    var prev = control.transform.localPosition;
                    if (origins.TryGetValue(control.name, out origin)) { control.transform.localPosition = origin; }
                    if (control.transform.localPosition != prev)
                    {
                        int cc = 0;
                        int ch = 0;
                        if (int.TryParse(control.name.Substring("Button".Length), out cc)) { cc += (64); if (cc >= (128)) { ch = (cc / (128)); cc -= (128) * (cc / (128)); } }
                        _SendMessage(1, 0xB0 + ch, cc, 0x00);
                    }
                }
                if (control.name.StartsWith("MatrixButton"))
                {
                    var prev = control.transform.localPosition;
                    if (origins.TryGetValue(control.name, out origin)) { control.transform.localPosition = origin; }
                    if (control.transform.localPosition != prev)
                    {
                        int cc = 0;
                        int ch = 0;
                        if (int.TryParse(control.name.Substring("MatrixButton".Length), out cc)) { cc -= (1); if (cc >= (128)) { ch = (cc / (128)); cc -= (128) * (cc / (128)); } }
                        _SendMessage(1, 0xBA + ch, cc, 0x00);
                    }
                }
                delta = 0.0f;
                first = true;
            }
        }
    }

    public int MIDI2ACK(int id, int msg, int x, int y)
    {
        return (y << 24) + (x << 16) + (msg << 8) + id;
    }

    public void Test()
    {
        MIDIEvent(MIDI2ACK(2, 0xBA, 0, 0));
    }

    public void Acknowledge(int ACK)
    {
        int id = (ACK) & 0xFF;
        int msg = (ACK >> 8) & 0xFF;
        int x = (ACK >> 16) & 0xFF;
        int y = (ACK >> 24) & 0xFF;
        Debug.Log("[ACK]: [" + id.ToString("X2") + ", " + msg.ToString("X2") + ", " + x.ToString("X2") + ", " + y.ToString("X2") + "]");
    }

    public void MIDIEvent(int data)
    {
        int id = (data) & 0xFF;
        int msg = (data >> 8) & 0xFF;
        int x = (data >> 16) & 0xFF;
        int y = (data >> 24) & 0xFF;
        Debug.Log("[EVNT]: [" + id.ToString("X2") + ", " + msg.ToString("X2") + ", " + x.ToString("X2") + ", " + y.ToString("X2") + "]");
        
        string obj = "";
        int cc = 0;
        GameObject go = null;

        if (msg == 0xB0)
        {
            cc = x - 65;
            obj += "Button" + cc.ToString();
            
        }
        if (msg == 0xB1)
        {
            cc = x + 62;
            obj += "Button" + cc.ToString();
        } 
        if (msg == 0xBA)
        {
            cc = x + 1;
            obj += "MatrixButton" + cc.ToString();
        }
        if (msg == 0xBB)
        {
            cc = x + 128;
            obj += "MatrixButton" + cc.ToString();
        }

        go = GameObject.Find(obj);
        if (go != null)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color c = new Color((y / 127.0f) * 255.0f, 0.0f, 0.0f);
                if ((msg == 0xB1) && (cc >= 62) && (cc <= 68)) { c.g = c.r; c.b = c.r; }
                renderer.material.SetColor("_Color", c);
                renderer.material.SetColor("_EmissionColor", c);
            }
            else { Debug.Log("[WARN]: Material Not Found"); }
        }
        else { Debug.Log("[WARN]: Object Not Found"); }
    }
}
