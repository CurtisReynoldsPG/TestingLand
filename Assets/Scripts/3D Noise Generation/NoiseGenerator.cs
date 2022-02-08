using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator: MonoBehaviour
{
    private struct PointData
    {
        public bool isPoint;
    }


    [SerializeField, Range(2,512)] private int _textureResolution;
    [SerializeField, Range(1, 64)] private int _numberOfPoints;

    [SerializeField] private FilterMode _filterMode;
    private PointData[,] _textureData;

    private Vector2[] _pointArray;

    private Texture2D _texture;


    private void Start()
    {
        InitalizeData();
    }

    private void Update()
    {
        //if (transform.hasChanged)
        //{
        //    transform.hasChanged = false;
        //    FillTexture();
        //}
    }

    private void InitalizeData()
    {
        if(_texture == null)
        {
            _texture = new Texture2D(_textureResolution, _textureResolution, TextureFormat.RGB24, false);
            _texture.name = "Procedural Texture";

            //Needed to prevent overflowing edges and smooth gradient
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.filterMode = _filterMode;
            _texture.anisoLevel = 9;

            GetComponent<MeshRenderer>().material.mainTexture = _texture;
        }

        //init texture Array
        _textureData = new PointData[_textureResolution, _textureResolution];
        _pointArray = new Vector2[16];
        GenerateCells();
        FillTexture();
    }



    public void FillTexture()
    {
        //checks to enable hot reload
        if (_texture.width != _textureResolution) _texture.Resize(_textureResolution, _textureResolution);
        if (_texture.filterMode != _filterMode) _texture.filterMode = _filterMode;

        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 point10 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));


        float stepSize = 1f / _textureResolution;
        for (int y = 0; y < _textureResolution; y++)
        {
            //Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            //Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < _textureResolution; x++)
            {
                //Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                if(_textureData[x,y].isPoint)
                {
                    _texture.SetPixel(x, y, Color.red);
                    continue;
                }
                _texture.SetPixel(x,y,ColorPixel(FindNearest(new Vector2(x,y))));

            }
            //_texture.SetPixel(0, 0, Color.black);
            //_texture.SetPixel(15, 0, Color.black);
            //_texture.SetPixel(0, 15, Color.black);
            //_texture.SetPixel(15, 15, Color.black);


        }
        _texture.Apply();
    }

    private void GenerateCells()
    {
        int step = _textureResolution / _numberOfPoints;

        int count = 0;

        for (int i = 0; i < step; i++)
        {
            for (int z = 0; z < step; z++)
            {
                int x = Random.Range(_numberOfPoints + (_numberOfPoints * i -1), (_numberOfPoints * i));
                int y = Random.Range(_numberOfPoints + (_numberOfPoints * z - 1), (_numberOfPoints * z));

                _textureData[x, y].isPoint = true;
                _pointArray[count] = new Vector2(x, y);
                count++;
            }
        }
    }

    private float FindNearest(Vector2 pos)
    {
        Vector2 shortestPos = Vector2.zero;
        float shortestDistance = float.MaxValue;
        float distance = 0;
        foreach (Vector2 point in _pointArray)
        {
            distance = Vector2.Distance(pos / _textureResolution, point / _textureResolution);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                shortestPos = point;
            }
        }
        Vector2 newPos = new Vector2((pos.x - _textureResolution/2 ) / _textureResolution, (pos.y - _textureResolution / 2 ) / _textureResolution);
        Vector2 newShortestPos = new Vector2((shortestPos.x - _textureResolution / 2) / _textureResolution, (shortestPos.y - _textureResolution / 2) / _textureResolution);
        //Debug.DrawLine(newPos, newShortestPos, Color.white, 10f);
        return shortestDistance;
    }


    private Color ColorPixel(float dist)
    {
        float min = 0.05f;
        float max = 0.2f;

        float clampedDist = Mathf.Clamp(dist, min, max);
        float value = (clampedDist - min) / (max - min);
        Debug.Log($"Initial Distanct {dist}, normalized disntance {value}");
        return new Color(value, value, value);
    }
}

//Range of .2 -> .8s