using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Calamitytwinklefragment.Content.TFTools
{
    class PredictionTF
    {
        public static Vector2 PredictionPositionTF(Vector2 position, Vector2 velocity, float time)
        {
            // 预判位置 = 当前位置 + 速度 * 时间
            Vector2 predictedPosition = position + velocity * time;
            return predictedPosition;
        }

        public static Vector2 YourTargetTF(Vector2 position1, Vector2 position2, float speed)
        {
            // 计算从位置1指向位置2的方向向量
            Vector2 direction = position2 - position1;

            // 标准化方向向量（使其长度为1）
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            // 将方向向量乘以速度值，得到最终向量
            Vector2 result = direction * speed;
            return result;
        }
    }
}
