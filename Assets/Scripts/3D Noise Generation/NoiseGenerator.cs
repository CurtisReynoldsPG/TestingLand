using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class NoiseGenerator: MonoBehaviour
{
    private struct PointData
    {
        public bool isPoint;
    }


    [SerializeField, Range(2,512)] private int _textureResolution;
    [SerializeField] private int _cellSize;

    [SerializeField] private float _minDistance = 0.03f;
    [SerializeField] private float _maxDistance = 0.25f;


    [SerializeField] private bool _drawPoints = false;
    [SerializeField] private bool _invertColour = false;

    [SerializeField] private FilterMode _filterMode;
    private PointData[,] _textureData;

    private Vector2[] _pointArray;
    private Vector2[] _leftPointArray;

    private Vector2[] _pointArrayAll;

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
        _pointArray = new Vector2[(_textureResolution / _cellSize) * (_textureResolution / _cellSize)];
        GenerateCells();
        GenerateTilingPoints();
        FillTexture();
    }



    public void FillTexture()
    {
        //checks to enable hot reload
        if (_texture.width != _textureResolution) _texture.Resize(_textureResolution, _textureResolution);
        if (_texture.filterMode != _filterMode) _texture.filterMode = _filterMode;

        float stepSize = 1f / _textureResolution;
        for (int y = 0; y < _textureResolution; y++)
        {
            for (int x = 0; x < _textureResolution; x++)
            {
                if(_drawPoints)
                {
                    if (_textureData[x, y].isPoint)
                    {
                        _texture.SetPixel(x, y, Color.red);
                        continue;
                    }
                }

                _texture.SetPixel(x,y,ColorPixel(FindNearest(new Vector2(x,y))));
            }
        }
        _texture.Apply();
        //SaveImage();
    }

    private void GenerateCells()
    {
        int step = _textureResolution / _cellSize;

        int count = 0;

        for (int i = 0; i < step; i++)
        {
            for (int z = 0; z < step; z++)
            {
                int x = Random.Range(_cellSize + (_cellSize * i -1), (_cellSize * i));
                int y = Random.Range(_cellSize + (_cellSize * z - 1), (_cellSize * z));

                _textureData[x, y].isPoint = true;
                _pointArray[count] = new Vector2(x, y);
                count++;
            }
        }
    }

    private void GenerateTilingPoints()
    {
        _pointArrayAll = new Vector2[_pointArray.Length * 9];

        Vector2 leftOffset = new Vector2( 0 - _textureResolution, 0);
        Vector2 rightOffset = new Vector2(0 + _textureResolution, 0);

        Vector2 topLeftOffset = new Vector2(0 - _textureResolution, 0 + _textureResolution);
        Vector2 topRightOffset = new Vector2(0 + _textureResolution, 0 + _textureResolution);

        Vector2 topOffset = new Vector2( 0 , 0 + _textureResolution);
        Vector2 bottomOffset = new Vector2( 0, 0 - _textureResolution);

        Vector2 bottomLeftOffset = new Vector2(0 - _textureResolution, 0 - _textureResolution);
        Vector2 bottomRightOffset = new Vector2(0 + _textureResolution, 0 - _textureResolution);

        int counter = 0;

        for (int i = 0; i < _pointArray.Length; i++)
        {
            //Generate center point
            _pointArrayAll[counter] = _pointArray[i];

            //Generate left point
            _pointArrayAll[counter + 1] = _pointArray[i] + leftOffset;

            //Generate topLeft point
            _pointArrayAll[counter + 2] = _pointArray[i] + topLeftOffset;

            //Generate top point
            _pointArrayAll[counter + 3] = _pointArray[i] + topOffset;

            //Generate topRight point
            _pointArrayAll[counter + 4] = _pointArray[i] + topRightOffset;

            //Generate right point
            _pointArrayAll[counter + 5] = _pointArray[i] + rightOffset;

            //Generate bottomRight
            _pointArrayAll[counter + 6] = _pointArray[i] + bottomRightOffset;

            //Generate bottom point
            _pointArrayAll[counter + 7] = _pointArray[i] + bottomOffset;

            //Generate bottom left
            _pointArrayAll[counter + 8] = _pointArray[i] + bottomLeftOffset;

            counter += 9;
        }
    }

    private float FindNearest(Vector2 pos)
    {
        Vector2 shortestPos = Vector2.zero;
        float shortestDistance = float.MaxValue;
        float distance = 0;
        foreach (Vector2 point in _pointArrayAll)
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
        return shortestDistance;
    }


    private Color ColorPixel(float dist)
    {
        float clampedDist = Mathf.Clamp(dist, _minDistance, _maxDistance);
        float value = (clampedDist - _minDistance) / (_maxDistance- _minDistance);
        if(_invertColour)
        {
            return InvertColor(new Color (value, value, value));
        }
        return new Color(value, value, value);
    }

    private Color InvertColor(Color color)
    {
        return new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b);
    }

    private void SaveImage()
    {
        byte[] bytes = _texture.EncodeToPNG();
        var dirPath = "C:/Users/Public/Repos/TestingLand/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
    }
}
