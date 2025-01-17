﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMapObjects
{
    public class moProjectionCS
    {
        //说明：
        //（1）本类型的所有算法均将地理坐标系的本初子午线默认为格林威治0度经线；
        //（2）仅实现了Lambert_Conformal_Conic_2SP、Gauss_Kruger、UTM投影下的地理坐标与投影坐标的互换

        #region 字段
        //常数
        private const double PI = Math.PI;              //圆周率
        private const double HALF_PI = PI * 0.5;        //1/2圆周率
        private const double TWO_PI = PI * 2.0;         //2倍圆周率
        private const double EPSILON = 0.0000000001;    //极小值
        //坐标系名称
        private string _ProjCSName = "";              //投影坐标系名称
        //地理坐标系参数
        private string _GeoCSName = "";             //地理坐标系名称
        private string _DatumName = "";             //大地基准面名称
        private string _SpheroidName = "";          //椭球体名称
        //Private _SpheroidType As SpheroidTypeConstant = SpheroidTypeConstant.Krassowsky_1940    ''椭球体类型
        private double _SemiMajor;                  //长半轴
        private double _SemiMinor;                  //短半轴
        private double _InverseFlattening;          //扁率倒数
        private string _PrimeMeridian = "";         //本初子午线
        //投影参数
        private moProjectionTypeConstant _ProjType; //投影类型
        private string _ProjName;                   //投影名称
        private double _CentralMeridian;            //中央经线（度）
        private double _OriginLatitude;             //原点纬度（度）
        private double _StandardParallelOne;        //第一标准纬线
        private double _StandardParallelTwo;        //第二标准纬线
        private double _ScaleFactor;                //投影比例因子
        private double _FalseEasting;               //东伪偏移值
        private double _FalseNorthing;              //北伪偏移值
        private moLinearUnitConstant _LinearUnit;   //投影后的长度单位

        //通用参数
        private double a, b, alpha;                 //长半轴，短半轴，扁率
        private double e, e2, e4, e6, e8;           //第一偏心率，及第一偏心率的平方、四次方、六次方、八次方
        private double ep, ep2;                     //第二偏心率，及第二偏心率的平方
        private double lambda0;                     //中央经线（弧度）
        private double phi0, phi1, phi2;            // 原点纬线，第一、第二标准纬线（弧度）
        //特殊参数-墨卡托投影参数
        private double ak;                          //没有使用
        //特殊参数-Lambert和Albers投影参数
        private double n, f0, r0, af0, _1n, e_2;
        //特殊参数-高斯克吕格（横轴墨卡托）投影参数
        private double m0;                          //没有使用
        private double _e, _e2, _e3, _e4;           //没有使用

        #endregion

        #region 构造函数

        /// <summary>
        /// 新建投影坐标系统
        /// </summary>
        /// <param name="projCSName">投影坐标系统名称</param>
        /// <param name="geoCSName">地理坐标系统名称</param>
        /// <param name="datumName">大地基准面名称</param>
        /// <param name="spheroidName">椭球体名称</param>
        /// <param name="semiMajor">椭球体长半轴</param>
        /// <param name="inverseFlattening">椭球体扁率倒数</param>
        /// <param name="projType">投影类型</param>
        /// <param name="originLatitude">原点纬度</param>
        /// <param name="centralMeridian">中央经线</param>
        /// <param name="falseEasting">东伪偏移</param>
        /// <param name="falseNorthing">北伪偏移</param>
        /// <param name="scaleFactor">比例因子</param>
        /// <param name="standardParallelOne">标准纬线1</param>
        /// <param name="standardParallelTwo">标准纬线2</param>
        /// <param name="linearUnit">线性单位</param>
        public moProjectionCS(String projCSName, String geoCSName, String datumName, string spheroidName, double semiMajor, double inverseFlattening,
            moProjectionTypeConstant projType, double originLatitude, double centralMeridian, double falseEasting, double falseNorthing, double scaleFactor,
                double standardParallelOne, double standardParallelTwo, moLinearUnitConstant linearUnit)
        {
            _ProjCSName = projCSName;
            _GeoCSName = geoCSName;
            _DatumName = datumName;
            _SpheroidName = spheroidName;
            _SemiMajor = semiMajor;
            _InverseFlattening = inverseFlattening;
            _SemiMinor = _SemiMajor - _SemiMajor / _InverseFlattening;
            _PrimeMeridian = "Greenwich 0";
            _ProjType = projType;
            _ProjName = _ProjType.ToString().Replace("_", " ");
            _OriginLatitude = originLatitude;
            _CentralMeridian = centralMeridian;
            _ScaleFactor = scaleFactor;
            _FalseEasting = falseEasting;
            _FalseNorthing = falseNorthing;
            _StandardParallelOne = standardParallelOne;
            _StandardParallelTwo = standardParallelTwo;
            _LinearUnit = linearUnit;

            //初始化其他变量
            InitializeVars();
        }

        #endregion

        #region 属性

        //获取投影坐标系统名称
        public string ProjCSName
        {
            get { return _ProjCSName; }
        }

        //获取地理坐标系统名称
        public string GeoCSName
        {
            get { return _GeoCSName; }
        }

        //获取大地基准面名称
        public string DatumName
        {
            get { return _DatumName; }
        }

        //获取椭球体名称
        public string SpheroidName
        {
            get { return _SpheroidName; }
        }

        //获取椭球体长半轴
        public double SemiMajor
        {
            get { return _SemiMajor; }
        }

        //获取椭球体短半轴
        public double SemiMinor
        {
            get { return _SemiMinor; }
        }

        //获取椭球体扁率倒数
        public double InverseFlattening
        {
            get { return _InverseFlattening; }
        }

        //获取本初子午线名称
        public string PrimeMeridian
        {
            get { return _PrimeMeridian; }
        }

        //获取投影类型
        public moProjectionTypeConstant ProjType
        {
            get { return _ProjType; }
        }

        //获取投影名称
        public string ProjName
        {
            get { return _ProjName; }
        }

        //获取中央经线（度）
        public double CentralMeridian
        {
            get { return _CentralMeridian; }
        }

        //获取原点纬度（度）
        public double OriginLatitude
        {
            get { return _OriginLatitude; }
        }

        //获取第一标准纬线（度）
        public double StandardParallelOne
        {
            get { return _StandardParallelOne; }
        }

        //获取第二标准纬线（度）
        public double StandardParallelTwo
        {
            get { return _StandardParallelTwo; }
        }

        //获取投影比例因子
        public double ScaleFactor
        {
            get { return _ScaleFactor; }
        }

        //获取东伪偏移值
        public double FalseEasting
        {
            get { return _FalseEasting; }
        }

        //获取北伪偏移值
        public double FalseNorthing
        {
            get { return _FalseNorthing; }
        }

        //获取投影后的长度单位
        public moLinearUnitConstant LinearUnit
        {
            get { return _LinearUnit; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 将经纬度转换为投影坐标
        /// </summary>
        /// <param name="LngLat"></param>
        /// <returns></returns>
        public moPoint TransferToProjCo(moPoint LngLat)
        {
            moPoint sResult;
            if (Math.Abs(LngLat.Y) >= 90.0 - EPSILON)
            {
                throw new Exception("纬度应该在-90.0-90.0之间且不能对极点投影！");
            }
            if (_ProjType == moProjectionTypeConstant.None)
                sResult = new moPoint(LngLat.X, LngLat.Y);
            else if (_ProjType == moProjectionTypeConstant.Mercator)
                throw new NotImplementedException();
            else if (_ProjType == moProjectionTypeConstant.UTM)
                sResult = UTMFwd(LngLat);
            else if (_ProjType == moProjectionTypeConstant.Gauss_Kruger)
                sResult = Gauss_KrugerFwd(LngLat);
            else if (_ProjType == moProjectionTypeConstant.Lambert_Conformal_Conic_2SP)
                sResult = LambertFwd(LngLat);
            else if (_ProjType == moProjectionTypeConstant.Albers_Equal_Area)
                throw new NotImplementedException();
            else
                throw new NotImplementedException();
            return sResult;
        }

        /// <summary>
        /// 将投影坐标转换为经纬度
        /// </summary>
        /// <param name="ProjCo"></param>
        /// <returns></returns>
        public moPoint TransferToLngLat(moPoint ProjCo)
        {
            moPoint sResult;
            if (_ProjType == moProjectionTypeConstant.None)
                sResult = new moPoint(ProjCo.X, ProjCo.Y);
            else if (_ProjType == moProjectionTypeConstant.Mercator)
                throw new NotImplementedException();
            else if (_ProjType == moProjectionTypeConstant.UTM)
                sResult = UTMInv(ProjCo);
            else if (_ProjType == moProjectionTypeConstant.Gauss_Kruger)
                sResult = Gauss_KrugerInv(ProjCo);
            else if (_ProjType == moProjectionTypeConstant.Lambert_Conformal_Conic_2SP)
                sResult = LambertInv(ProjCo);
            else if (_ProjType == moProjectionTypeConstant.Albers_Equal_Area)
                throw new NotImplementedException();
            else
                throw new NotImplementedException();
            return sResult;
        }

        /// <summary>
        /// 把以米为单位的数值转换为本投影系统中长度单位
        /// </summary>
        /// <param name="DataIn"></param>
        /// <returns></returns>
        public double ToUnits(double dataIn)
        {
            double sDataOut = 0;
            if (_LinearUnit == moLinearUnitConstant.Millimeter)
                sDataOut = dataIn * 1000;
            else if (_LinearUnit == moLinearUnitConstant.Centimeter)
                sDataOut = dataIn * 100;
            else if (_LinearUnit == moLinearUnitConstant.Decimeter)
                sDataOut = dataIn * 10;
            else if (_LinearUnit == moLinearUnitConstant.Meter)
                sDataOut = dataIn;
            else if (_LinearUnit == moLinearUnitConstant.Kilometer)
                sDataOut = dataIn * 0.001;
            return sDataOut;
        }

        /// <summary>
        /// 把以本投影系统中长度单位为单位的数值转换为以米为单位的数值
        /// </summary>
        /// <param name="dataIn"></param>
        /// <returns></returns>
        public double ToMeters(double dataIn)
        {
            double sDataOut = 0;
            if (_LinearUnit == moLinearUnitConstant.Millimeter)
                sDataOut = dataIn * 0.001;
            else if (_LinearUnit == moLinearUnitConstant.Centimeter)
                sDataOut = dataIn * 0.01;
            else if (_LinearUnit == moLinearUnitConstant.Decimeter)
                sDataOut = dataIn * 0.1;
            else if (_LinearUnit == moLinearUnitConstant.Meter)
                sDataOut = dataIn;
            else if (_LinearUnit == moLinearUnitConstant.Kilometer)
                sDataOut = dataIn * 1000;
            return sDataOut;
        }

        #endregion

        #region 私有函数

        private void InitializeVars()
        {
            //中央经线（弧度）
            lambda0 = ToRadians(_CentralMeridian);
            //原点纬线
            phi0 = ToRadians(_OriginLatitude);
            //长半轴
            a = _SemiMajor;
            //短半轴
            b = _SemiMinor;
            //扁率
            alpha = 1 / _InverseFlattening;
            //第一偏心率的计算
            double sRatio = b / a;
            e = Math.Sqrt(1.0 - sRatio * sRatio);
            e2 = Math.Pow(e, 2);
            e4 = Math.Pow(e, 4);
            e6 = Math.Pow(e, 6);
            e8 = Math.Pow(e, 8);
            //第二偏心率的计算
            sRatio = a / b;
            ep = Math.Sqrt(sRatio * sRatio - 1.0);
            ep2 = ep * ep;
            //根据投影类型设置特殊变量的值
            if (_ProjType == moProjectionTypeConstant.Mercator)
            { throw new NotImplementedException(); }
            else if (_ProjType == moProjectionTypeConstant.Gauss_Kruger)
            {
                m0 = mlfn(phi0);
                _e = (a - b) / (a + b);
                _e2 = Math.Pow(_e, 2);
                _e3 = Math.Pow(_e, 3);
                _e4 = Math.Pow(_e, 4);
            }
            else if (_ProjType == moProjectionTypeConstant.UTM)
            { }
            else if (_ProjType == moProjectionTypeConstant.Lambert_Conformal_Conic_2SP)
            {
                double m1, m2;
                double t, t1, t2;
                phi1 = ToRadians(_StandardParallelOne);
                phi2 = ToRadians(_StandardParallelTwo);

                if (Math.Abs(phi1 + phi2) < EPSILON)
                { throw new ArgumentException("位于赤道两侧的标准纬线不能相同！"); }
                e_2 = 0.5 * e;
                double cosPhi, _eSinPhi, con;
                _eSinPhi = e * Math.Sin(phi0);
                t = Math.Tan(0.5 * (HALF_PI - phi0)) / Math.Pow((1.0 - _eSinPhi) / (1.0 + _eSinPhi), 0.5 * e);
                cosPhi = Math.Cos(phi1);
                con = Math.Sin(phi1);
                _eSinPhi = e * con;
                m1 = cosPhi / Math.Sqrt(1.0 - _eSinPhi * _eSinPhi);
                t1 = Math.Tan(0.5 * (HALF_PI - phi1)) / Math.Pow((1.0 - _eSinPhi) / (1.0 + _eSinPhi), 0.5 * e);
                cosPhi = Math.Cos(phi2);
                _eSinPhi = e * Math.Sin(phi2);
                m2 = cosPhi / Math.Sqrt(1.0 - _eSinPhi * _eSinPhi);
                t2 = Math.Tan(0.5 * (HALF_PI - phi2)) / Math.Pow((1.0 - _eSinPhi) / (1.0 + _eSinPhi), 0.5 * e);

                if (Math.Abs(phi1 - phi2) > EPSILON)
                {
                    n = Math.Log(m1 / m2) / Math.Log(t1 / t2);
                }
                else
                {
                    n = con;
                }
                if (n > 0.0)
                { _1n = 1.0 / n; }
                f0 = m1 / (n * Math.Pow(t1, n));
                af0 = a * f0;
                r0 = af0 * Math.Pow(t, n);
            }
            else if (_ProjType == moProjectionTypeConstant.Albers_Equal_Area)
            { throw new NotImplementedException(); }
            else
            { throw new NotImplementedException(); }
        }

        //横轴墨卡托公式的一部分
        private double mlfn(double phi)
        {
            return a * ((3072.0 - 768.0 * e2 - 144.0 * e4 - 60.0 * e6) * phi -
                (1152.0 * e2 + 288.0 * e4 + 135.0 * e6) * Math.Sin(2.0 * phi) +
                (180.0 * e4 + 135.0 * e6) * Math.Sin(4.0 * phi) - 35.0 * e6 * Math.Sin(6.0 * phi)) / 3072.0;
        }

        //横轴墨卡托反算公式的一部分
        private double blfn(double phi)
        {
            return phi + ((2304.0 * _e - 1296.0 * _e3) * Math.Sin(2.0 * phi) +
                (2016.0 * _e2 - 2640.0 * _e4) * Math.Sin(4.0 * phi) +
                2416.0 * _e3 * Math.Sin(6.0 * phi) + 3291.0 * _e4 * Math.Sin(8.0 * phi)) / 1536.0;
        }

        //将度转换为弧度
        private double ToRadians(double degrees)
        {
            return degrees * PI / 180.0;
        }

        //将弧度转换为度
        private double ToDegrees(double radians)
        {
            return (radians * 180.0) / PI;
        }


        //根据x的正负返回－1或者1
        private double Sign(double x)
        {
            if (x < 0.0)
                return -1;
            else
                return 1;
        }

        //将经纬度转换为投影坐标——Lambert
        private moPoint LambertFwd(moPoint LngLat)
        {
            double lng = ToRadians(LngLat.X);
            double lat = ToRadians(LngLat.Y);
            double eSinLat, t;
            double r, theta;
            double con = Math.Abs(Math.Abs(lat) - HALF_PI);
            if (con > EPSILON)
            {
                eSinLat = e * Math.Sin(lat);
                t = Math.Tan(0.5 * (HALF_PI - lat)) / Math.Pow((1.0 - eSinLat) / (1.0 + eSinLat), 0.5 * e);
                r = af0 * Math.Pow(t, n);
            }
            else
            {
                con = lat * n;
                if (con <= 0)
                { throw new Exception(); }
                r = 0;
            }
            theta = n * (lng - lambda0);
            r = ToUnits(r);
            double x = _FalseEasting + r * Math.Sin(theta);
            double y = _FalseNorthing + r0 - r * Math.Cos(theta);
            moPoint sResult = new moPoint(x, y);
            return sResult;
        }

        //将投影坐标转换为经纬度——Lambert
        private moPoint LambertInv(moPoint ProjCo)
        {
            double lng = double.NaN;
            double lat = double.NaN;
            double dx = ProjCo.X - _FalseEasting;
            double dy = r0 - ProjCo.Y + _FalseNorthing;
            double r, theta, t;
            double con = Sign(n);
            r = Sign(n) * Math.Sqrt(dx * dx + dy * dy);
            theta = 0.0;
            if (r != 0)
            { theta = Math.Atan2((con * dx), con * dy); }
            if ((r != 0) || (n > 0.0))
            {
                r = ToMeters(r);
                t = Math.Pow((r / af0), _1n);
                lat = HALF_PI - 2.0 * Math.Atan(t);
                double eSinLat, dphi;
                Int32 I;
                for (I = 0; I <= 4; I++)
                {
                    eSinLat = e * Math.Sin(lat);
                    dphi = HALF_PI - 2.0 * Math.Atan(t * Math.Pow((1.0 - eSinLat) / (1.0 + eSinLat), e_2)) - lat;
                    lat += dphi;
                    if (Math.Abs(dphi) <= EPSILON)
                    { break; }
                }
            }
            else
                lat = -HALF_PI;
            lng = theta / n + lambda0;
            moPoint sResult = new moPoint(ToDegrees(lng), ToDegrees(lat));
            return sResult;
        }

        //将经纬度转换为投影坐标——UTM
        private moPoint UTMFwd(moPoint LngLat)
        {
            double lng = ToRadians(LngLat.X);
            double lat = ToRadians(LngLat.Y);

            double sinLat = Math.Sin(lat);
            double cosLat = Math.Cos(lat);
            double al = cosLat * (lng - lambda0);
            double al2 = al * al;
            double al4 = Math.Pow(al, 4);
            double c = ep2 * cosLat * cosLat;
            double tanLat = Math.Tan(lat);
            double t = tanLat * tanLat;
            double t2 = t * t;
            double n = a / Math.Sqrt(1.0 - e2 * sinLat * sinLat);
            double ml0 = mlfn(lat);

            double x = _ScaleFactor * n * al * (120.0 + 20.0 * (1 - t + c) * al2 +
                (5.0 - 18.0 * t + t2 + 72.0 * c - 58.0 * ep2) * al2 * al2) / 120.0;
            x = ToUnits(_FalseEasting + x);
            double y = _ScaleFactor * (ml0 - m0 + n * tanLat * al2 * (360.0 +
                (150.0 - 30.0 * t + 270.0 * c + 120.0 * c * c) * al2 +
                (61.0 - 58.0 * t + t2 + 600.0 * c - 330.0 * ep2) * al4) / 720.0);
            y = ToUnits(_FalseNorthing + y);
            moPoint sResult = new moPoint(x, y);
            return sResult;
        }

        //将投影坐标转换为经纬度——UTM
        private moPoint UTMInv(moPoint ProjCo)
        {
            double dx = ToMeters(ProjCo.X) - _FalseEasting;
            double dy = ToMeters(ProjCo.Y) - _FalseNorthing;

            double ma = (m0 + dy / _ScaleFactor) / a;
            double _phi = ma * 256.0 / (256.0 - 64.0 * e2 - 12.0 * e4 - 5.0 * e6);
            _phi = blfn(_phi);

            if (Math.Abs(_phi) < HALF_PI)
            {
                double sinPhi = Math.Sin(_phi);
                double cosPhi = Math.Cos(_phi);
                double c = ep2 * cosPhi * cosPhi;
                double c2 = c * c;
                double tanPhi = Math.Tan(_phi);
                double t = tanPhi * tanPhi;
                double t2 = Math.Pow(tanPhi, 4);
                double con = 1.0 - e2 * sinPhi * sinPhi;
                double n = a / Math.Sqrt(con);
                double r = n * (1.0 - e2) / con;
                double d = dx / (n * _ScaleFactor);
                double d2 = d * d;
                double d4 = Math.Pow(d, 4);

                double lng = lambda0 + (d * (120.0 - (20.0 + 40.0 * t + 20.0 * c) * d2 +
                    (5.0 - 2.0 * c + 28.0 * t - 3.0 * c2 + 8.0 * ep2 + 24.0 * t2) * d4) / (cosPhi * 120.0));

                double lat = _phi - ((n * tanPhi * d2 / r) * (360.0 - (150.0 + 90.0 * t + 300.0 * c - 120.0 * c2 -
                    270.0 * ep2) * d2 + (61.0 + 90.0 * t + 298.0 * c + 45.0 * t2 - 252.0 * ep2 - 3.0 * c2) * d4) / 720.0);
                moPoint sResult = new moPoint(ToDegrees(lng), ToDegrees(lat));
                return sResult;
            }
            else
            {
                moPoint sResult = new moPoint(_CentralMeridian, ToDegrees(HALF_PI * Sign(dy)));
                return sResult;
            }
        }

        //将经纬度转换为投影坐标——Gauss_Kruger
        private moPoint Gauss_KrugerFwd(moPoint LngLat)
        {
            double lng = ToRadians(LngLat.X);
            double lat = ToRadians(LngLat.Y);

            double sinLat = Math.Sin(lat);
            double cosLat = Math.Cos(lat);
            double al = cosLat * (lng - lambda0);
            double al2 = al * al;
            double al4 = Math.Pow(al, 4);
            double c = ep2 * cosLat * cosLat;
            double tanLat = Math.Tan(lat);
            double t = tanLat * tanLat;
            double t2 = t * t;
            double n = a / Math.Sqrt(1.0 - e2 * sinLat * sinLat);
            double ml0 = mlfn(lat);
            double t2c = t2 * c;

            double x = _ScaleFactor * n * al * (120.0 + 20.0 * (1 - t + c) * al2 +
                (5.0 - 18.0 * t + t2 + 14.0 * c - 58.0 * t2c) * al2 * al2) / 120.0;
            x = ToUnits(_FalseEasting + x);
            double y = _ScaleFactor * (ml0 - m0 + n * tanLat * al2 * (360.0 +
                (150.0 - 30.0 * t + 270.0 * c + 120.0 * c * c) * al2 +
                (61.0 - 58.0 * t + t2 + 270.0 * c - 330.0 * t2c) * al4) / 720.0);
            y = ToUnits(_FalseNorthing + y);
            moPoint sResult = new moPoint(x, y);
            return sResult;
        }

        //将投影坐标转换为经纬度——Gauss_Kruger
        private moPoint Gauss_KrugerInv(moPoint ProjPoint)
        {
            double dx = ToMeters(ProjPoint.X) - _FalseEasting;
            double dy = ToMeters(ProjPoint.Y) - _FalseNorthing;

            double ma = (m0 + dy / _ScaleFactor) / a;
            double _phi = ma * 256.0 / (256.0 - 64.0 * e2 - 12.0 * e4 - 5.0 * e6);
            _phi = blfn(_phi);

            if (Math.Abs(_phi) < HALF_PI)
            {
                double sinPhi = Math.Sin(_phi);
                double cosPhi = Math.Cos(_phi);
                double c = ep2 * cosPhi * cosPhi;
                double c2 = c * c;
                double tanPhi = Math.Tan(_phi);
                double t = tanPhi * tanPhi;
                double t2 = Math.Pow(tanPhi, 4);
                double con = 1.0 - e2 * sinPhi * sinPhi;
                double n = a / Math.Sqrt(con);
                double r = n * (1.0 - e2) / con;
                double d = dx / (n * _ScaleFactor);
                double d2 = d * d;
                double d4 = Math.Pow(d, 4);
                double t2c = t2 * c;

                double lng = lambda0 + (d * (120.0 - (20.0 + 40.0 * t + 20.0 * c) * d2 +
                    (5.0 + 28.0 * t + 6.0 * c + 8.0 * t2c + 24.0 * t2) * d4) / (cosPhi * 120.0));

                double lat = _phi - ((n * tanPhi * d2 / r) * (360.0 - (150.0 + 90.0 * t + 30.0 * c -
                    270.0 * t2c) * d2 + (61.0 + 90.0 * t + 45.0 * t2) * d4) / 720.0);
                moPoint sResult = new moPoint(ToDegrees(lng), ToDegrees(lat));
                return sResult;
            }
            else
            {
                moPoint sResult = new moPoint(_CentralMeridian, ToDegrees(HALF_PI * Sign(dy)));
                return sResult;
            }
        }

        #endregion
    }
}
