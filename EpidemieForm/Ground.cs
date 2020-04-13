using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpidemieForm
{
    public enum Border { UP, DOWN, LEFT, RIGHT }
    public enum State { Sain, Malade, Immunite }

    public class Ground
    {
        public static double Width, Height, BulletRadius, Speed;
        public static int DelaiContagion = 15;
        public Unit[] AllBullets;
        public static Random random = new Random();

        public Ground(int N)
        {
            Unit.idb = 0;
            AllBullets = new Unit[N];
            for (int i = 0; i < N; ++i)
            {
                double alpha = Math.PI * 2 * random.NextDouble();
                var u = new Unit();
                u.x = (Width - 2 * BulletRadius) * random.NextDouble() + BulletRadius;
                u.y = (Height - 2 * BulletRadius) * random.NextDouble() + BulletRadius;
                u.vx = Speed * Math.Cos(alpha);
                u.vy = Speed * Math.Sin(alpha);

                AllBullets[i] = u;
            }
        }

        public bool CheckBorderCollision(Unit u)
        {
            if (u.x < BulletRadius / 2 && u.vx < 0) return u.bounceBorder(Border.LEFT);
            if (u.y < BulletRadius / 2 && u.vy < 0) return u.bounceBorder(Border.UP);
            if (u.x > Width - BulletRadius / 2 && u.vx > 0) return u.bounceBorder(Border.RIGHT);
            if (u.y > Height - BulletRadius / 2 && u.vy > 0) return u.bounceBorder(Border.DOWN);

            return false;
        }

        public void CheckBulletCollision(Unit u)
        {
            double dist = Width + Height;
            int id = -1;

            for (int i = u.id + 1; i < AllBullets.Length; ++i)
            {
                var u1 = AllBullets[i];
                if (u1.collid || u.id == u1.lastCollid) continue;
                var d0 = u.distance(u1);
                if (d0 < dist)
                {
                    dist = d0;
                    id = u1.id;
                }
            }

            if (id != -1)
            {
                var u1 = AllBullets[id];
                if (dist < BulletRadius * 2)
                {
                    u.collid = u1.collid = true;
                    bounceBullet(u, u1);
                }
            }
        }

        public void CheckCollision()
        {
            foreach (var u in AllBullets)
                u.collid = false;

            foreach (var u in AllBullets)
            {
                if (u.collid) continue;
                if (CheckBorderCollision(u))
                {
                    u.collid = true;
                    continue;
                }

                CheckBulletCollision(u);
            }
        }

        public void Update()
        {
            CheckCollision();

            foreach (var u in AllBullets)
                u.Update();
        }

        // Non realistic choc
        public void bounceBullet(Unit u1, Unit u2)
        {
            double mcoeff = 2.0;
            double nx = u1.x - u2.x;
            double ny = u1.y - u2.y;
            double nxnysquare = nx * nx + ny * ny;
            double dvx = u1.vx - u2.vx;
            double dvy = u1.vy - u2.vy;
            double product = (nx * dvx + ny * dvy) / (nxnysquare * mcoeff);
            double fx = nx * product;
            double fy = ny * product;

            u1.vx -= fx;
            u1.vy -= fy;
            u2.vx += fx;
            u2.vy += fy;

            double impulse = Math.Sqrt(fx * fx + fy * fy);
            double coeff = 1.0;
            if (impulse > 1.0e-6 && impulse < Speed)
            {
                coeff = Speed / impulse;
            }

            fx = fx * coeff;
            fy = fy * coeff;

            u1.vx -= fx;
            u1.vy -= fy;
            u2.vx += fx;
            u2.vy += fy;

            // Speed normalize
            double c1 = Speed / Math.Sqrt(u1.vx * u1.vx + u1.vy * u1.vy);
            double c2 = Speed / Math.Sqrt(u2.vx * u2.vx + u2.vy * u2.vy);
            u1.vx *= c1;
            u1.vy *= c1;
            u2.vx *= c2;
            u2.vy *= c2;

            u1.lastCollid = u2.id;
            u2.lastCollid = u1.id;

            if (u1.state == State.Sain && u2.state == State.Malade) u1.state = State.Malade;
            else if (u2.state == State.Sain && u1.state == State.Malade) u2.state = State.Malade;
        }
    }

    public class Unit
    {
        public static int idb = 0;
        public int id, lastCollid = -1;
        public double x, y, vx, vy, radius = Ground.BulletRadius;
        public bool collid = false;

        public State state = State.Sain;
        public int delaiContagion = Ground.DelaiContagion;

        public Unit()
        {
            id = idb++;
            lastCollid = id;
        }

        public double distance(Unit u) => Math.Sqrt((x - u.x) * (x - u.x) + (y - u.y) * (y - u.y));

        public void Update()
        {
            x += vx;
            y += vy;

            if (state == State.Malade)
                --delaiContagion;

            if (delaiContagion == 0) state = State.Immunite;
        }

        public bool bounceBorder(Border border)
        {
            //if (border == Border.UP || border == Border.DOWN) vy *= -1;
            //else if (border == Border.LEFT || border == Border.RIGHT) vx *= -1;
            if (border == Border.DOWN) y = Ground.BulletRadius / 2;
            else if (border == Border.UP) y = Ground.Height - Ground.BulletRadius / 2;
            else if (border == Border.LEFT) x = Ground.Width - Ground.BulletRadius / 2;
            else if (border == Border.RIGHT) x = Ground.BulletRadius / 2;

            return true;
        }

        public void moveTo(Unit p, double dist)
        {
            double d = distance(p);
            if (d < 1e-6)
                return;

            double coef = dist / d;
            x += (p.x - x) * coef;
            y += (p.y - y) * coef;
        }

    }
}
