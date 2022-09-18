﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GSCEditor
{
    class Couple : UniversalObject
    {
        Primitive first;
        Primitive second;

        int typeOperation;

        public Couple(Primitive first, Primitive second, int type) {
            this.first = first;
            this.second = second;
            typeOperation = type;
        }

        public void render(Graphics g, Pen pen)
        {
            int minY = first.getMinY();
            if (minY > second.getMinY())
                minY = second.getMinY();

            int maxY = second.getMaxY();
            if (maxY < first.getMaxY())
                maxY = first.getMaxY();

            for (int Y = minY + 1; Y < maxY; Y++)
            {
                Sides borders = getActualSides(Y);

                List<float> Xrl = borders.getLeft();
                List<float> Xrr = borders.getRight();

                // отрисовка
                for (int i = 0; i < Xrr.Count; i++)
                {
                    if (Xrl[i] < Xrr[i])
                        g.DrawLine(pen, Xrl[i], Y, Xrr[i], Y);
                }
            }
        }

        public Sides getActualSides(int Y)
        {
            Sides firstBorders = first.getActualSides(Y);
            Sides secondBorders = second.getActualSides(Y);

            List<float> Xal = firstBorders.getLeft();
            List<float> Xar = firstBorders.getRight();
            List<float> Xbl = secondBorders.getLeft();
            List<float> Xbr = secondBorders.getRight();

            // заполнение рабочего списка строки
            List<float[]> M = new List<float[]>();
            for (int i = 0; i < Xal.Count; i++)
                M.Add(new float[] { Xal[i], 2 });

            for (int i = 0; i < Xar.Count; i++)
                M.Add(new float[] { Xar[i], -2 });

            for (int i = 0; i < Xbl.Count; i++)
                M.Add(new float[] { Xbl[i], 1 });

            for (int i = 0; i < Xbr.Count; i++)
                M.Add(new float[] { Xbr[i], -1 });
            

            sortM(M);

            // весы для расчета Q
            int[] setQ;
            switch (typeOperation)
            {
                case 1: setQ = new int[] { 1, 2 }; break; // симметрическая разность        
                default:
                   setQ = new int[] { 2, 2 }; break; // разность   
            }

            List<float> left = new List<float>();
            List<float> right = new List<float>();

            float Q = 0;
            for (int i = 0; i < M.Count; i++)
            {
                float nQ = Q + M[i][1]; // определение суммы и проверка на соответствие весам
                                        // добавление необходимых отрезков
                if ((Q < setQ[0] || Q > setQ[1]) && nQ >= setQ[0] && nQ <= setQ[1])
                {
                    left.Add(M[i][0]);
                }
                if (Q >= setQ[0] && Q <= setQ[1] && (nQ < setQ[0] || nQ > setQ[1]))
                {
                    right.Add(M[i][0]);
                }
                Q = nQ;
            }
            return new Sides(left, right);
        }

        private void sortM(List<float[]> M)
        {
            int len = M.Count;
            // пузырьковая
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len - 1; j++)
                {
                    if (M[j][0] > M[j + 1][0])
                    {
                        float[] temp = M[j];
                        M[j] = M[j + 1];
                        M[j + 1] = temp;
                    }
                }
            }

        }

        public int getMaxY()
        {
            int maxY = second.getMaxY();
            if (maxY < first.getMaxY())
                maxY = first.getMaxY();
            return maxY;
        }

        public int getMinY()
        {
            int minY = first.getMinY();
            if (minY > second.getMinY())
                minY = second.getMinY();
            return minY;
        }

        public bool isInside(int x, int y)
        {
            Sides borders = getActualSides(y);

            for (int i = 0; i < borders.getLeft().Count; i++) {
                if (borders.getLeft()[i] <= x && borders.getRight()[i] >= x)
                    return true;
            }
            return false;
        }

        public void move(float dx, float dy)
        {
            first.move(dx, dy);
            second.move(dx, dy);
        }

        public void reset()
        {
            first.reset();
            second.reset();
        }

        public void rotate(float angle, PointF relatePoint)
        {
            first.rotate(angle, relatePoint);
            second.rotate(angle, relatePoint);
        }

        public void scale(float s, PointF relatePoint)
        {
            first.scale(s, relatePoint);
            second.scale(s, relatePoint);
        }

        public PointF getCenter()
        {
            PointF firstCenter = first.getCenter();
            PointF secondCenter = second.getCenter();

            float x = (firstCenter.X + secondCenter.X)/2;
            float y = (firstCenter.Y + secondCenter.Y) / 2;

            return new PointF(x, y);
        }

        public void mirror(PointF origin)
        {
            first.mirror(origin);
            second.mirror(origin);
        }
    }
}
