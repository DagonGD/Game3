using Microsoft.Xna.Framework;

namespace Game3
{
    /// <summary>
    /// Камера
    /// </summary>
    public interface ICamera
    {
        /// <summary>Матрица вида</summary>
        Matrix View { get;}
        /// <summary>Матрица проекции</summary>
        Matrix Proj { get;}
    }
}
