using System;
using System.Collections.Generic;
using UnityEngine;

public class FigureDetection : MonoBehaviour {

    private int PIXELS_PER_GRID = 6;
    private int GRID_NUMBER = 180;
    Texture2D pngTexture;

    

    public struct CustomPixel
    {
        public int posX;
        public int posY;

        public static bool operator ==(CustomPixel c1, CustomPixel c2)
        {
            return c1.posX.Equals(c2.posX) && c1.posY.Equals(c2.posY);
        }
        public static bool operator !=(CustomPixel c1, CustomPixel c2)
        {
            return !c1.posX.Equals(c2.posX) || !c1.posY.Equals(c2.posY);
        }

    };

    public struct CustomCell
    {
        public int gridId;
        public CustomPixel p1, p2, p3, p4;

       
    }   

    private Texture2D image;
    public CustomCell[,] grid;

    private List<List<CustomCell>> gridsAdyacentes = new List<List<CustomCell>>();
    public bool gridContine;

    public int dibujosSize = 0;

    public List<Texture2D> toReturn;
    public List<float[]> toReturnPositions;

    public List<Texture2D> StartDetection(Texture2D _image, int cellCount, int cellSize) {

        toReturn = new List<Texture2D>();
        this.image = _image;

        GRID_NUMBER = cellCount;
        PIXELS_PER_GRID = cellSize;

        grid = new CustomCell[GRID_NUMBER, GRID_NUMBER];

        //Creacion de CustomGrids
        int counterX = 0, counterY = 0, gridCounter = 0;

        for(int y = 0; y < GRID_NUMBER; y++)
        {
            for(int x = 0; x < GRID_NUMBER ; x++)
            {
                grid[x,y] = new CustomCell();
                grid[x, y].gridId = gridCounter;

            }
        }


        //Por cada cell del grid, setteamos 4 esquinas y sus posiciones
        for(int y = image.height; y > 0; y -= PIXELS_PER_GRID)
        {
            for(int x = 0; x < image.width; x += PIXELS_PER_GRID)
            {
                grid[counterX, counterY].p1.posX = x;
                grid[counterX, counterY].p1.posY = y;

                grid[counterX, counterY].p2.posX = x + PIXELS_PER_GRID;
                grid[counterX,counterY].p2.posY = y;

                grid[counterX, counterY].p3.posX = x;
                grid[counterX, counterY].p3.posY = y - PIXELS_PER_GRID;

                grid[counterX, counterY].p4.posX = x + PIXELS_PER_GRID;
                grid[counterX, counterY].p4.posY = y - PIXELS_PER_GRID;

                counterX++;
            }

            counterX = 0;
            counterY++;
        }

        //---------
        pngTexture = new Texture2D(image.width, image.height, TextureFormat.ARGB32, false);
        pngTexture.SetPixels(image.GetPixels());

        dibujosSize = 0;
        //Recorremos el grid y miramos por cada cell si hay dibujo en esa celda o no.
        for (int y = 0; y < GRID_NUMBER; y++)
        {
            for (int x = 0; x < GRID_NUMBER; x++)
            {
               
                DetectarDibujo(x, y);

            }
        }

        toReturnPositions = new List<float[]>();
        for(int i = 0; i < gridsAdyacentes.Count; i++)
        {

            float equis = (gridsAdyacentes[i][0].p1.posX + gridsAdyacentes[i][0].p2.posX) / 2;
            float ey = (gridsAdyacentes[i][0].p1.posY + gridsAdyacentes[i][0].p3.posY) / 2;
            float[] temp = new float[2];
            temp[0] = equis;
            temp[1] = ey;

            toReturnPositions.Add(temp);
            ExtraerDibujo(i);

        }

        return toReturn;

    }

    public List<float[]> GetReturnPositions()
    {
        return this.toReturnPositions;
    }

    public bool firstTime = true;
    //Si se detecta dibujo, se guarda la celda.
    void DetectarDibujo(int x, int y)
    {
        bool _break = false;
        
        for(int _y = grid[x,y].p1.posY; _y > grid[x,y].p3.posY; _y--)
        {
            for(int _x = grid[x,y].p1.posX; _x < grid[x,y].p2.posX; _x++)
            {
                if(image.GetPixel(_x,_y).grayscale <= 0.5f)
                {
                    if (firstTime)
                    {
                        firstTime = false;
                        gridsAdyacentes.Insert(dibujosSize, new List<CustomCell>());

                        gridsAdyacentes[dibujosSize].Add(grid[x, y]);


                    } else if (thisGridAdyacente(grid[x, y], gridsAdyacentes[dibujosSize]))
                    {
                        gridsAdyacentes[dibujosSize].Add(grid[x, y]);

                    }
                    else if (gridsAdyacentes.Count <= dibujosSize + 1 && gridsAdyacentes[dibujosSize] != null && !thisGridAdyacente(grid[x, y], gridsAdyacentes[dibujosSize]))
                    {
                        bool newDraw = true;
                        for (int i = 0; i < gridsAdyacentes.Count; i++)
                        {
                            if (thisGridAdyacente(grid[x, y], gridsAdyacentes[i]))
                            {
                                gridsAdyacentes[i].Add(grid[x, y]);
                                newDraw = false;
                                break;
                            }

                        }

                        if (newDraw)
                        {
                            dibujosSize++;
                            gridsAdyacentes.Insert(dibujosSize, new List<CustomCell>());

                            gridsAdyacentes[dibujosSize].Add(grid[x, y]);

                        }
                    }                  

                    _break = true;
                    break;
                }
            }

            if (_break) break;

        }
    }

    bool thisGridAdyacente(CustomCell cell, List<CustomCell> grid)
    {
        for(int i = 0; i < grid.Count; i++)
        {
            if(cell.p1 == grid[i].p1 | cell.p1 == grid[i].p2 | cell.p1 == grid[i].p3 | cell.p1 == grid[i].p4 |
               cell.p2 == grid[i].p1 | cell.p2 == grid[i].p2 | cell.p2 == grid[i].p3 | cell.p2 == grid[i].p4 |
               cell.p3 == grid[i].p1 | cell.p3 == grid[i].p2 | cell.p3 == grid[i].p3 | cell.p3 == grid[i].p4 |
               cell.p4 == grid[i].p1 | cell.p4 == grid[i].p2 | cell.p4 == grid[i].p3 | cell.p4 == grid[i].p4)
            {
                return true;
            }
        }

        return false;

    }

    void ExtraerDibujo(int pos)
    {
        CustomCell left = new CustomCell();
        left.p1.posX = 999999;
        CustomCell right = new CustomCell();
        CustomCell up = new CustomCell();
        CustomCell down = new CustomCell();

        for (int i = 0; i < gridsAdyacentes[pos].Count; i++)
        {

            if (gridsAdyacentes[pos][i].p1.posX < left.p1.posX)
            {
                left = gridsAdyacentes[pos][i];
            }
            if (gridsAdyacentes[pos][i].p2.posX > right.p2.posX)
            {
                right = gridsAdyacentes[pos][i];
            }
        }
        up = gridsAdyacentes[pos][0];
        down = gridsAdyacentes[pos][gridsAdyacentes[pos].Count - 1];

        CustomCell topleft = new CustomCell();
        topleft.p1.posX = left.p1.posX;
        topleft.p1.posY = up.p1.posY;
        topleft.p2.posX = left.p2.posX;
        topleft.p2.posY = up.p2.posY;
        topleft.p3.posX = left.p3.posX;
        topleft.p3.posY = up.p3.posY;
        topleft.p4.posX = left.p4.posX;
        topleft.p4.posY = up.p4.posY;

        CustomCell toprigth = new CustomCell();
        toprigth.p1.posX = right.p1.posX;
        toprigth.p1.posY = up.p1.posY;
        toprigth.p2.posX = right.p2.posX;
        toprigth.p2.posY = up.p2.posY;
        toprigth.p3.posX = right.p3.posX;
        toprigth.p3.posY = up.p3.posY;
        toprigth.p4.posX = right.p4.posX;
        toprigth.p4.posY = up.p4.posY;

        CustomCell botleft = new CustomCell();
        botleft.p1.posX = left.p1.posX;
        botleft.p1.posY = down.p1.posY;
        botleft.p2.posX = left.p2.posX;
        botleft.p2.posY = down.p2.posY;
        botleft.p3.posX = left.p3.posX;
        botleft.p3.posY = down.p3.posY;
        botleft.p4.posX = left.p4.posX;
        botleft.p4.posY = down.p4.posY;

        CustomCell botright = new CustomCell();
        botright.p1.posX = right.p1.posX;
        botright.p1.posY = down.p1.posY;
        botright.p2.posX = right.p2.posX;
        botright.p2.posY = down.p2.posY;
        botright.p3.posX = right.p3.posX;
        botright.p3.posY = down.p3.posY;
        botright.p4.posX = right.p4.posX;
        botright.p4.posY = down.p4.posY;

        pngTexture.Apply();

        int Ancho = (toprigth.p2.posX - topleft.p1.posX);
        int Alto = (topleft.p1.posY - botright.p3.posY);
        Texture2D newTexture = new Texture2D(Ancho, Alto, TextureFormat.ARGB32, false);

        newTexture.SetPixels(image.GetPixels(botleft.p1.posX, botleft.p3.posY, Ancho,Alto));

        newTexture.Apply();

        toReturn.Add(newTexture);

    }

    void OnDrawGizmos()
    {
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Gizmos.DrawLine(new Vector2(grid[x, y].p1.posX, grid[x, y].p1.posY), new Vector2(grid[x, y].p2.posX, grid[x, y].p2.posY));
                Gizmos.DrawLine(new Vector2(grid[x, y].p1.posX, grid[x, y].p1.posY), new Vector2(grid[x, y].p3.posX, grid[x, y].p3.posY));
                Gizmos.DrawLine(new Vector2(grid[x, y].p2.posX, grid[x, y].p2.posY), new Vector2(grid[x, y].p4.posX, grid[x, y].p4.posY));
                Gizmos.DrawLine(new Vector2(grid[x, y].p3.posX, grid[x, y].p3.posY), new Vector2(grid[x, y].p4.posX, grid[x, y].p4.posY));

            }

        }

    }
}
