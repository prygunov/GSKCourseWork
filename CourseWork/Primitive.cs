using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;


namespace GSCEditor
{
    internal class Primitive : UniversalObject
    {
        private List<PointF> points = new List<PointF>();
        private Color color;
        private float[,] modificationMatrix;
        private int mode = 0; // режимы 0 - многоугольник, 1 - прямая, 2 - кривая

        public Primitive(List<PointF> points, Color color)
        {
            this.points = points.ToList();
            if (points.Count == 2)
                mode = 1; // если точки две - прямая
            this.color = color;
            reset();
        }

        //устанавливает режим примитива
        public void setMode(int mode)
        {
            this.mode = mode;
        }

        //возвращает режим примитива
        public int getMode()
        {
            return mode;
        }

        //применяет матрицу преобразований к текущей
        void applyMatrix(float[,] matrix)
        {
            modificationMatrix = multiplyMatrix(modificationMatrix, matrix);
        }

        // представляет координаты вершин в виде матрицы
        public float[,] getAsMatrix()
        {
            float[,] polygonMatrix = new float[points.Count, 3]; // 3 столбца
            for (int i = 0; i < points.Count; i++)
            {
                polygonMatrix[i, 0] = points[i].X;
                polygonMatrix[i, 1] = points[i].Y;
                polygonMatrix[i, 2] = 1;
            }
            return polygonMatrix;
        }

        //сброс матрицы преобразований, единичная матрица
        public void reset()
        {
            modificationMatrix = new float[,] { { 1,0,0},
                                                { 0,1,0},
                                                { 0,0,1} };
        }

        //функция перемножения матриц, возвращает результат
        float[,] multiplyMatrix(float[,] first, float[,] second)
        {
            int rows = first.GetLength(0);
            int cols = first.GetLength(1);
            float[,] result = new float[rows, cols];
            for (int firstRow = 0; firstRow < first.Length / 3; firstRow++)
            {
                for (int secondColumn = 0; secondColumn < 3; secondColumn++)
                {
                    int secondRow = 0;
                    float num = 0f;
                    for (int firstColumn = 0; firstColumn < 3; firstColumn++)
                    {
                        num += first[firstRow, firstColumn] * second[secondRow, secondColumn];
                        secondRow++;
                    }
                    result[firstRow, secondColumn] = num;
                }

            }

            return result;
        }

        //отрисовка примитива
        public void render(Graphics g, Pen pen)
        {
            Color cache = pen.Color;
            pen.Color = color;

            List<PointF> points = getActualPoints();//получаем вершины после матрицы текущих преобразований
            if (points.Count > 1)
                if (mode == 0)
                {
                    //получаем минимальную и максимальную строки для отрисовки в этом диапазоне
                    int yMin = Math.Max(getMinY(), 0);
                    int yMax = Math.Min(getMaxY(), 1080);

                    //перебирая строки 
                    for (int Y = yMin; Y < yMax; Y++)
                    {
                        //получаем координаты левых и правых границ примитива в этой строке
                        Sides sides = getActualSides(Y);
                        List<float> left = sides.getLeft();
                        List<float> right = sides.getRight();
                        for (int i = 0; i < left.Count; i++)
                            g.DrawLine(pen, left[i], Y, right[i], Y); // зарисовываем от левых до правых
                    }
                }
                else if (mode == 1)
                {
                    //просто прямая
                    g.DrawLine(pen, points[0], points[1]);
                }
                else if (mode == 2)
                {
                    List<PointF> p = getBeziePoints(points);

                    for (int i = 1; i < p.Count; i++)
                    {
                        g.DrawLine(pen, p[i - 1], p[i]);
                    }
                }
            pen.Color = cache;
        }

        // Кривая Безье
        public List<PointF> getBeziePoints(List<PointF> points)
        {
            List<PointF> bezie = new List<PointF>();
            const double dt = 0.001f;
            double t = dt;
            double j;

            int n = points.Count - 1;
            double[] facts = new double[n + 1];
            facts[0] = 1;
            for (int i = 1; i <= n; i++)
            {
                facts[i] = facts[i - 1] * i;
            }

            double until = 1 + dt / 2;
            while (t < until)
            {
                float xt = 0, yt = 0;
                int i = 0;
                while (i <= n)
                {
                    j = (Math.Pow(t, i) * Math.Pow(1 - t, n - i) * facts[n] / (facts[i] * facts[n - i]));
                    xt = (float)(xt + points[i].X * j);
                    yt = (float)(yt + points[i].Y * j);
                    i++;
                }
                bezie.Add(new PointF(xt, yt));
                t += dt;
            }
            return bezie;
        }

        public int getMinY()
        {
            List<PointF> points = getActualPoints();
            int minY = (int)points[0].Y;
            foreach (PointF p in points)
            {
                if (p.Y < minY)
                    minY = (int)p.Y;
            }
            return minY;
        }

        public int getMaxY()
        {
            List<PointF> points = getActualPoints();
            int maxY = (int)points[0].Y;
            foreach (PointF p in points)
            {
                if (p.Y > maxY)
                    maxY = (int)p.Y;
            }
            return maxY;
        }

        public int getMaxYIndex()
        {
            List<PointF> points = getActualPoints();
            int maxY = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].Y > points[maxY].Y)
                    maxY = i;
            }
            return maxY;
        }

        // проверка, точка внутри примитива или нет (захват примитива)
        public bool isInside(int x, int y)
        {
            List<PointF> points = getActualPoints();
            if (points.Count == 2)
                if (isBetween(y, points[0].Y, points[1].Y, 2) || isBetween(y, points[1].Y, points[0].Y, 2))
                    return distanceToStraight(x, y, points[0], points[1]) < 5;
                else return false;
            else if (mode == 2)
            {
                List<PointF> p = getBeziePoints(points);

                for (int i = 1; i < p.Count; i++)
                {
                    if (((y > p[i - 1].Y && y < p[i].Y) || (y < p[i - 1].Y && y > p[i].Y)))
                        if (nearlyEqual(getX(y, p[i - 1].X, p[i - 1].Y, p[i].X, p[i].Y), x, 5))
                            return true; // захват кривой
                }
                return false;

            }

            Sides sides = getActualSides(y);

            for (int i = 0; i < sides.getLeft().Count; i++)
            {
                if (isBetween(x, sides.getLeft()[i], sides.getRight()[i], 0.1f))
                    return true;
            }
            return false;
        }

        //проверка, внутри ли значение заданного интервала, с погрешностью е
        private bool isBetween(float value, float fborder, float sborder, float e)
        {
            if (fborder > sborder) {
                float b = fborder;
                fborder = sborder;
                sborder = b;
            }

            return (value > fborder - e) && (value < sborder + e);
        }

        // расстояние до прямой
        private double distanceToStraight(int x, int y, PointF first, PointF second)
        {
            return Math.Abs((second.Y - first.Y) * x - (second.X - first.X) * y + second.X * first.Y - second.Y * first.X) /
                (Math.Sqrt(Math.Pow(second.Y - first.X, 2) + Math.Pow(second.X - first.X, 2)));
        }

        //равны ли с погрешностью e два значения
        public static bool nearlyEqual(float a, float b, float epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }

        // плоско-параллельное перемещение
        public void move(float dx, float dy)
        {
            applyMatrix(new float[,] { { 1, 0, 0},
                                         { 0, 1, 0},
                                         { dx, dy, 1} });
        }

        // вращение относительно заданной точки
        public void rotate(float rad, PointF origin)
        {
            move(-origin.X, -origin.Y);

            float sin = (float)Math.Sin(rad);
            float cos = (float)Math.Cos(rad);

            applyMatrix(new float[,] { { cos, sin, 0},
                                        { -sin, cos, 0},
                                        { 0, 0, 1} });

            move(origin.X, origin.Y);
        }

        // масштабирование относительно заданной точки
        public void scale(float s, PointF origin)
        {
            move(-origin.X, -origin.Y);
            applyMatrix(new float[,] { { s, 0, 0},
                                        { 0, s, 0},
                                        { 0, 0, 1} });
            move(origin.X, origin.Y);
        }

        // отражает объект  от горизонтальной прямой  
        public void mirror(PointF origin)
        {
            move(-origin.X, -origin.Y);
            applyMatrix(new float[,] { { 1, 0, 0},
                                        { 0, -1, 0},
                                        { 0, 0, 1} });
            move(origin.X, origin.Y);
        }

        private List<PointF> getActualPoints()
        {
            List<PointF> points = new List<PointF>();

            // вычисляем новые точки
            float[,] polygonMatrix = multiplyMatrix(getAsMatrix(), modificationMatrix);

            for (int i = 0; i < this.points.Count; i++)
            {
                points.Add(new PointF(polygonMatrix[i, 0], polygonMatrix[i, 1]));
            }

            return points;
            //возвращаем просчитанные ранее точки
        }

        //вычисление центра фигуры
        public PointF getCenter()
        {
            List<PointF> points = getActualPoints();

            float x = 0;
            float y = 0;
            foreach (PointF point in points)
            {
                x += point.X;
                y += point.Y;
            }
            return new PointF(x / points.Count, y / points.Count);
        }

        //получение левых и правых границ примитива для сечения Y
        public Sides getActualSides(int Y)
        {
            List<float> Xl = new List<float>();
            List<float> Xr = new List<float>();

            List<PointF> points = getActualPoints();
            bool cw = orientation();
            for (int i = 0; i < points.Count; i++)
            {
                int k = 0;
                if (i < points.Count - 1)
                    k = i + 1;

                if (((points[i].Y < Y) && (points[k].Y >= Y)) ||
                    ((points[i].Y >= Y) && (points[k].Y < Y)))
                {
                    float x = Primitive.getX(Y, points[i].X, points[i].Y, points[k].X, points[k].Y);
                    // в зависимости от направления стороны могут меняться,
                    // необходимо учитывать направление перебора вершин
                    if (!cw)
                    {
                        if (points[k].Y < points[i].Y)
                            Xr.Add(x); //движимся вниз
                        else
                            Xl.Add(x);
                    }
                    else
                    {
                        if (points[k].Y < points[i].Y)
                            Xl.Add(x);
                        else
                            Xr.Add(x); //движимся вверх
                    }
                }
            }

            Xl.Sort();
            Xr.Sort();

            return new Sides(Xl, Xr);
        }
        
        //возвращает X от Y заданной двумя точками прямой 
        public static float getX(float Y, float x1, float y1, float x2, float y2)
        {
            return ((Y - y1) * (x2 - x1) / (y2 - y1)) + x1;
        }

        //направление перебора
        private bool orientation()
        {
            int maxIndex = getMaxYIndex();
            int prevTriangle = maxIndex - 1;
            int nextTriangle = maxIndex + 1;

            if (prevTriangle == -1)
                prevTriangle = points.Count - 1;
            if (nextTriangle == points.Count)
                nextTriangle = 0;

            float s = getSquare(points[prevTriangle].X, points[prevTriangle].Y, points[maxIndex].X, points[maxIndex].Y, points[nextTriangle].X, points[nextTriangle].Y);
            // по "часовой" площадь отрицательная

            return s < 0; // cw значит "по часовой"
        }
        private float getSquare(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            return (-x1 * y2 - x2 * y3 - x3 * y1 + y1 * x2 + y2 * x3 + y3 * x1) / 2;
        }
    }
}