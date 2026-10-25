namespace VisionApp.Core.Models;

/* Một điểm trên ảnh, đơn vị pixel. Gốc (0, 0) ở góc trên bên trái, trục Y hướng xuống dưới. */
public readonly record struct Point2D(double X, double Y);
