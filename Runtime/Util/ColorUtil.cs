using UnityEngine;

namespace PLATEAU.Util
{
    public static class ColorUtil
    {
        /// <summary> Color32->Color変換 </summary>
        public static Color ToColor(this Color32 self)
        {
            return new Color(self.r / 255f, self.g / 255f, self.b / 255f, self.a / 255f);
        }

        /// <summary>
        /// HSV -> RGB変換
        /// </summary>
        /// <param name="h">色相. 0 ~ 360</param>
        /// <param name="s">彩度. 0 ~ 255</param>
        /// <param name="v">明度. 0 ~ 255</param>
        /// <param name="alpha">0 ~ 255</param>
        /// <returns></returns>
        public static Color Hsv2Rgb(float h, float s, float v, float alpha = 1.0f)
        {
            int Hi = Mathf.FloorToInt(h / 60.0f) % 6;
            float f = h / 60.0f - Hi;
            v = v / 255f;
            s = s / 255f;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            switch (Hi)
            {
                case 0:
                    return new Color(v, t, p, alpha);
                case 1:
                    return new Color(q, v, p, alpha);
                case 2:
                    return new Color(p, v, t, alpha);
                case 3:
                    return new Color(p, q, v, alpha);
                case 4:
                    return new Color(t, p, v, alpha);
                case 5:
                    return new Color(v, p, q, alpha);
            }

            // ここには来ない
            return new Color(v / 255, v / 255, v / 255, alpha);
        }


        /// <summary> r値を設定した値を返す(非破壊) </summary>
        public static Color PutR(this Color self, float r)
        {
            return new Color(r, self.g, self.b, self.a);
        }

        /// <summary> g値を設定した値を返す(非破壊) </summary>
        public static Color PutG(this Color self, float g)
        {
            return new Color(self.r, g, self.b, self.a);
        }

        /// <summary> b値を設定した値を返す(非破壊) </summary>
        public static Color PutB(this Color self, float b)
        {
            return new Color(self.r, self.g, b, self.a);
        }

        /// <summary> a値を設定した値を返す(非破壊) </summary>
        public static Color PutA(this Color self, float a)
        {
            return new Color(self.r, self.g, self.b, a);
        }

    }
}