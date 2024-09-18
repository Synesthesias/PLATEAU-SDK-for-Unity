using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PLATEAU.RoadNetwork.Graph
{
    /// <summary>
    /// ���H�^�C�v
    /// </summary>
    [Flags]
    public enum RRoadTypeMask
    {
        /// <summary>
        /// �����Ȃ�
        /// </summary>
        Empty = 0,
        /// <summary>
        /// �ԓ�
        /// </summary>
        Road = 1 << 0,
        /// <summary>
        /// ����
        /// </summary>
        SideWalk = 1 << 1,
        /// <summary>
        /// ����������
        /// </summary>
        Median = 1 << 2,
        /// <summary>
        /// �������H
        /// </summary>
        HighWay = 1 << 3,
        /// <summary>
        /// �s���Ȓl
        /// </summary>
        Undefined = 1 << 4,
        /// <summary>
        /// �S�Ă̒l
        /// </summary>
        All = ~0
    }

    public static class RRoadTypeEx
    {
        /// <summary>
        /// �ԓ�����
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsRoad(this RRoadTypeMask self)
        {
            return (self & RRoadTypeMask.Road) != 0;
        }

        /// <summary>
        /// ��ʓ��H
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsHighWay(this RRoadTypeMask self)
        {
            return (self & RRoadTypeMask.HighWay) != 0;
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsSideWalk(this RRoadTypeMask self)
        {
            return (self & RRoadTypeMask.SideWalk) != 0;
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsMedian(this RRoadTypeMask self)
        {
            return (self & RRoadTypeMask.Median) != 0;
        }

        /// <summary>
        /// self��flag�̂ǂꂩ�������Ă��邩�ǂ���
        /// </summary>
        /// <param name="self"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool HasAnyFlag(this RRoadTypeMask self, RRoadTypeMask flag)
        {
            return (self & flag) != 0;
        }
    }

    /// <summary>
    /// �ړ_
    /// </summary>
    [Serializable]
    public class RVertex : ARnParts<RVertex>
    {
        //----------------------------------
        // start: �t�B�[���h
        //----------------------------------

        /// <summary>
        /// �ڑ���
        /// </summary>
        private HashSet<REdge> edges = new HashSet<REdge>();

        /// <summary>
        /// �ʒu
        /// </summary>
        [field: SerializeField]
        public Vector3 Position { get; set; }

        //----------------------------------
        // start: �t�B�[���h
        //----------------------------------

        /// <summary>
        /// �ڑ���
        /// </summary>
        public IReadOnlyCollection<REdge> Edges => edges;

        public RRoadTypeMask TypeMask
        {
            get
            {
                var ret = RRoadTypeMask.Empty;
                foreach (var edge in Edges)
                {
                    foreach (var face in edge.Faces)
                        ret |= face.RoadTypes;
                }

                return ret;
            }
        }


        public IEnumerable<RFace> GetFaces()
        {
            foreach (var edge in Edges)
            {
                foreach (var face in edge.Faces)
                    yield return face;
            }
        }

        public RVertex(Vector3 v)
        {
            Position = v;
        }

        /// <summary>
        /// ��{�Ăяo���֎~. �ڑ��Ӓǉ�
        /// </summary>
        /// <param name="edge"></param>
        public void AddEdge(REdge edge)
        {
            if (edges.Contains(edge))
                return;

            edges.Add(edge);
        }

        /// <summary>
        /// ��{�Ăяo���֎~. �ڑ��Ӎ폜
        /// </summary>
        /// <param name="edge"></param>
        public void RemoveEdge(REdge edge)
        {
            edges.Remove(edge);
        }

        /// <summary>
        /// �������g���O��.
        /// removeEdge = true�̎��͎����������Ă���ӂ��폜����
        /// </summary>
        public void DisConnect(bool removeEdge = false)
        {
            if (removeEdge)
            {
                // �����������Ă���ӂ��폜����
                foreach (var e in Edges.ToList())
                    e.DisConnect();
            }
            else
            {
                // �����������Ă���ӂ��玩�����폜����
                foreach (var e in Edges.ToList())
                    e.RemoveVertex(this);
            }

        }

        /// <summary>
        /// �������g���������邤����, ���܂ł������Ȃ���͎c���悤�ɂ���
        /// </summary>
        public void DisConnectWithKeepLink()
        {
            var neighbors = GetNeighborVertices().ToList();

            // �����ƌq�����Ă���ӂ͈�U�폜
            foreach (var e in Edges.ToList())
                e.DisConnect();

            // �\��Ȃ���
            for (var i = 0; i < neighbors.Count; i++)
            {
                var v0 = neighbors[i];
                if (v0 == null)
                    continue;
                for (var j = i; j < neighbors.Count; ++j)
                {
                    var v1 = neighbors[j];
                    if (v1 == null)
                        continue;
                    if (v0.IsNeighbor(v1))
                        continue;
                    // �V�����ӂ��쐬����
                    var _ = new REdge(v0, v1);
                }
            }
        }

        /// <summary>
        /// �אڒ��_���擾
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RVertex> GetNeighborVertices()
        {
            foreach (var edge in Edges)
            {
                if (edge.V0 == this)
                {
                    Assert.IsTrue(edge.V1 != this);
                    yield return edge.V1;
                }
                else
                {
                    Assert.IsTrue(edge.V0 != this);
                    yield return edge.V0;
                }
            }
        }

        /// <summary>
        /// other�Ƃ̒��ڂ̕ӂ������Ă��邩
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsNeighbor(RVertex other)
        {
            return Edges.Any(e => e.Vertices.Contains(other));
        }


        /// <summary>
        /// ���g��dst�Ƀ}�[�W����
        /// </summary>
        public void MergeTo(RVertex dst, bool checkEdgeMerge = true)
        {
            // src�Ɍq�����Ă���ӂɕύX��ʒm����
            foreach (var e in Edges.ToList())
            {
                e.ChangeVertex(this, dst);
            }
            // �����̐ڑ��͉�������
            DisConnect();
            if (checkEdgeMerge == false)
                return;
            // �������_�������Ă���ӂ��}�[�W����
            var queue = dst.Edges.ToList();
            while (queue.Any())
            {
                var edge = queue[0];
                queue.RemoveAt(0);
                for (var i = 0; i < queue.Count;)
                {
                    if (edge.IsSameVertex(queue[i]))
                    {
                        queue[i].MergeTo(edge);
                        queue.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }


    /// <summary>
    /// ��
    /// </summary>
    [Serializable]
    public class REdge
        : ARnParts<REdge>
    {
        public enum VertexType
        {
            V0,
            V1,
        }

        //----------------------------------
        // start: �t�B�[���h
        //----------------------------------

        /// <summary>
        /// �ڑ���
        /// </summary>
        private HashSet<RFace> faces = new HashSet<RFace>();

        /// <summary>
        /// �\�����_(2��)
        /// </summary>
        [SerializeField]
        private RVertex[] vertices = new RVertex[2];

        //----------------------------------
        // end: �t�B�[���h
        //----------------------------------

        /// <summary>
        /// �J�n�_
        /// </summary>
        public RVertex V0 => GetVertex(VertexType.V0);

        /// <summary>
        /// �I���_
        /// </summary>
        public RVertex V1 => GetVertex(VertexType.V1);

        /// <summary>
        /// �ڑ���
        /// </summary>
        public IReadOnlyCollection<RFace> Faces => faces;

        /// <summary>
        /// �\�����_(2��)
        /// </summary>
        public IReadOnlyList<RVertex> Vertices => vertices;

        /// <summary>
        /// �L���ȕӂ��ǂ���. 2�̒��_�����݂��āA���قȂ邩�ǂ���
        /// </summary>
        public bool IsValid => V0 != null && V1 != null && V0 != V1;

        public REdge(RVertex v0, RVertex v1)
        {
            SetVertex(VertexType.V0, v0);
            SetVertex(VertexType.V1, v1);
        }

        /// <summary>
        /// �אڂ��Ă���Edge���擾
        /// </summary>
        /// <returns></returns>
        public IEnumerable<REdge> GetNeighborEdges()
        {
            if (V0 != null)
            {
                foreach (var e in V0.Edges)
                {
                    if (e != this)
                        yield return e;
                }
            }

            if (V1 != null && V1 != V0)
            {
                foreach (var e in V1.Edges)
                {
                    if (e != this)
                        yield return e;
                }
            }
        }

        /// <summary>
        /// ���_�m�[�h�擾
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public RVertex GetVertex(VertexType type)
        {
            return vertices[(int)type];
        }

        /// <summary>
        /// ���_�m�[�h�������ւ�
        /// </summary>
        /// <param name="type"></param>
        /// <param name="vertex"></param>
        public void SetVertex(VertexType type, RVertex vertex)
        {
            var old = vertices[(int)type];
            if (old == vertex)
                return;

            old?.RemoveEdge(this);
            vertices[(int)type] = vertex;
            vertex?.AddEdge(this);
        }

        /// <summary>
        /// ���_from -> to�ɕύX����
        /// from�������Ă��Ȃ��ꍇ�͖���
        /// �ύX�������ʗ����Ƃ�to�ɂȂ�ꍇ�͐ڑ������������
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void ChangeVertex(RVertex from, RVertex to)
        {
            if (V0 == from)
            {
                // �����Ƃ�to�ɂȂ�ꍇ�͐ڑ������������
                if (V1 == to)
                    DisConnect();
                else
                    SetVertex(VertexType.V0, to);
            }

            if (V1 == from)
            {
                // �����Ƃ�to�ɂȂ�ꍇ�͐ڑ������������
                if (V0 == to)
                    DisConnect();
                else
                    SetVertex(VertexType.V1, to);
            }
        }

        /// <summary>
        /// ��{�Ăяo���֎~. �אږʒǉ�
        /// </summary>
        /// <param name="face"></param>
        public void AddFace(RFace face)
        {
            if (faces.Contains(face))
                return;
            faces.Add(face);
        }

        /// <summary>
        /// ��{�Ăяo���֎~. �ʂ̂Ȃ��������
        /// </summary>
        /// <param name="face"></param>
        public void RemoveFace(RFace face)
        {
            faces.Remove(face);
        }

        /// <summary>
        /// ���_���폜����
        /// </summary>
        /// <param name="vertex"></param>
        public void RemoveVertex(RVertex vertex)
        {
            if (V0 == vertex)
                SetVertex(VertexType.V0, null);
            if (V1 == vertex)
                SetVertex(VertexType.V1, null);
        }

        /// <summary>
        /// �����̐ڑ�����������
        /// </summary>
        public void DisConnect()
        {
            // �q�Ɏ����̐ڑ�����������悤�ɓ`����
            foreach (var v in vertices.ToList())
            {
                v?.RemoveEdge(this);
            }

            // �e�Ɏ����̐ڑ�����������悤�ɓ`����
            foreach (var p in faces.ToList())
            {
                p?.RemoveEdge(this);
            }

            faces.Clear();
            Array.Fill(vertices, null);
        }

        /// <summary>
        /// v��2�ɕ�������, ����edge��V0->v, �V����Edge��v->V1�ɂȂ�. �V����Edge��Ԃ�
        /// </summary>
        /// <param name="v"></param>
        public REdge SplitEdge(RVertex v)
        {
            var lastV1 = V1;
            SetVertex(VertexType.V1, v);
            var newEdge = new REdge(v, lastV1);
            foreach (var p in Faces)
            {
                p.AddEdge(newEdge);
            }

            return newEdge;
        }

        /// <summary>
        /// �������_���Q�Ƃ��Ă��邩�ǂ���. (�����͖��Ȃ�)
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public bool IsSameVertex(RVertex v0, RVertex v1)
        {
            return (V0 == v0 && V1 == v1) || (V0 == v1 && V1 == v0);
        }

        public bool IsSameVertex(REdge other)
        {
            return IsSameVertex(other.V0, other.V1);
        }

        /// <summary>
        /// other�Ƌ��L���Ă��钸�_�����邩�ǂ���
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsShareAnyVertex(REdge other)
        {
            return IsShareAnyVertex(other, out _);
        }

        /// <summary>
        /// other�Ƌ��L���Ă��钸�_�����邩�ǂ���
        /// </summary>
        /// <param name="other"></param>
        /// <param name="sharedVertex"></param>
        /// <returns></returns>
        public bool IsShareAnyVertex(REdge other, out RVertex sharedVertex)
        {
            if (V0 == other.V0 || V1 == other.V0)
            {
                sharedVertex = other.V0;
                return true;
            }

            if (V0 == other.V1 || V1 == other.V1)
            {
                sharedVertex = other.V1;
                return true;
            }

            sharedVertex = null;
            return false;
        }

        /// <summary>
        /// vertex�Ɣ��Α��̒��_���擾����. vertex���܂܂�Ă��Ȃ��ꍇ��null��Ԃ�
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="opposite"></param>
        /// <returns></returns>
        public bool TryGetOppositeVertex(RVertex vertex, out RVertex opposite)
        {
            if (V0 == vertex)
            {
                opposite = V1;
                return true;
            }

            if (V1 == vertex)
            {
                opposite = V0;
                return true;
            }

            opposite = null;
            return false;
        }

        /// <summary>
        /// vertex�Ɣ��Α��̒��_���擾����. vertex���܂܂�Ă��Ȃ��ꍇ��null��Ԃ�
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public RVertex GetOppositeVertex(RVertex vertex)
        {
            if (TryGetOppositeVertex(vertex, out var opposite))
                return opposite;
            return null;
        }

        /// <summary>
        /// ���g��dst�Ƀ}�[�W����
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="checkFaceMerge"></param>
        public void MergeTo(REdge dst, bool checkFaceMerge = true)
        {
            var addFaces =
                Faces.Where(poly => dst.Faces.Contains(poly) == false).ToList();
            foreach (var p in addFaces)
            {
                p.ChangeEdge(this, dst);
            }

            // �Ō�Ɏ����̐ڑ��͉�������
            DisConnect();

            if (checkFaceMerge == false)
                return;
            var queue = dst.Faces.ToList();
            while (queue.Any())
            {
                var poly = queue[0];
                queue.RemoveAt(0);
                for (var i = 0; i < queue.Count;)
                {
                    if (poly.IsSameEdges(queue[i]))
                    {
                        // �ӂ͑S�ē����Ȃ̂ō������̂��߈ړ������͍s��Ȃ�
                        queue[i].TryMergeTo(poly, false);
                        queue.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// �ӂ̏W��
    /// </summary>
    [Serializable]
    public class RFace : ARnParts<RFace>
    {
        //----------------------------------
        // start: �t�B�[���h
        //----------------------------------
        /// <summary>
        /// �\����\��
        /// </summary>
        [field: SerializeField]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// �Ή�����CityObjectGroup
        /// </summary>
        [SerializeField]
        private PLATEAUCityObjectGroup cityObjectGroup = null;

        /// <summary>
        /// ���H�^�C�v
        /// </summary>
        [field: SerializeField]
        public RRoadTypeMask RoadTypes { get; set; }

        /// <summary>
        /// LodLevel
        /// </summary>
        [field: SerializeField]
        public int LodLevel { get; set; }

        /// <summary>
        /// �e�O���t
        /// </summary>
        private RGraph graph = null;

        /// <summary>
        /// �\����
        /// </summary>
        private HashSet<REdge> edges = new HashSet<REdge>();

        //----------------------------------
        // end: �t�B�[���h
        //----------------------------------

        /// <summary>
        /// �\����
        /// </summary>
        public IReadOnlyCollection<REdge> Edges => edges;

        /// <summary>
        /// �ڑ���
        /// </summary>
        public RGraph Graph => graph;

        /// <summary>
        /// �֘A����CityObjectGroup
        /// </summary>
        public PLATEAUCityObjectGroup CityObjectGroup => cityObjectGroup;

        // �L���ȃ|���S�����ǂ���
        public bool IsValid => edges.Count > 0;

        public RFace(RGraph graph, PLATEAUCityObjectGroup cityObjectGroup, RRoadTypeMask roadType, int lodLevel)
        {
            RoadTypes = roadType;
            LodLevel = lodLevel;
            this.cityObjectGroup = cityObjectGroup;
            this.graph = graph;
        }

        /// <summary>
        /// �Ӓǉ�
        /// </summary>
        /// <param name="edge"></param>
        public void AddEdge(REdge edge)
        {
            if (edges.Contains(edge))
                return;

            edges.Add(edge);
            edge.AddFace(this);
        }

        /// <summary>
        /// �e�O���t�폜
        /// </summary>
        /// <param name="g"></param>
        public void RemoveGraph(RGraph g)
        {
            if (Graph == g)
                graph = null;
        }

        /// <summary>
        /// �e�O���t�����ւ�
        /// </summary>
        /// <param name="g"></param>
        public void SetGraph(RGraph g)
        {
            if (graph == g)
                return;

            graph?.RemoveFace(this);
            graph = g;
        }

        ///// <summary>
        ///// ��{�ĂԂ̋֎~. edge��pos�̌��ɒǉ�����
        ///// </summary>
        ///// <param name="edge"></param>
        ///// <param name="pos"></param>
        //public void InsertEdge(REdge edge, REdge pos)
        //{
        //    if (edges.Contains(edge))
        //        return;
        //    var index = edges.IndexOf(pos);
        //    edges.Insert(index + 1, edge);
        //    edge.AddFace(this);
        //}

        /// <summary>
        /// �Ӎ폜
        /// </summary>
        /// <param name="edge"></param>
        public void RemoveEdge(REdge edge)
        {
            edges.Remove(edge);
            // �q���玩�����폜
            edge.RemoveFace(this);
        }

        /// <summary>
        /// �ӂ̕ύX
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void ChangeEdge(REdge from, REdge to)
        {
            RemoveEdge(from);
            AddEdge(to);
        }

        /// <summary>
        /// ����Edge�ō\������Ă��邩�ǂ���
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameEdges(RFace other)
        {
            if (edges.Count != other.edges.Count)
                return false;
            return other.Edges.All(e => Edges.Contains(e));
        }

        /// <summary>
        /// ��{�Ăяo���֎~. ���g��dst�Ƀ}�[�W����.
        /// CityObjectGroup���قȂ�ꍇ�̓}�[�W�ł��Ȃ�.
        /// moveEdge = false�̎��͎��g��Edges�͈ړ����Ȃ�.
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="moveEdge"></param>
        public bool TryMergeTo(RFace dst, bool moveEdge = true)
        {
            if (dst.CityObjectGroup && CityObjectGroup && dst.CityObjectGroup != CityObjectGroup)
            {
                Debug.LogWarning($"CityObjectGroup���قȂ�Face�͓����ł��܂���. {CityObjectGroup} != {dst.CityObjectGroup}");
                return false;
            }

            dst.RoadTypes |= RoadTypes;
            dst.cityObjectGroup = CityObjectGroup;
            dst.LodLevel = Mathf.Max(dst.LodLevel, LodLevel);
            foreach (var e in Edges)
                dst.AddEdge(e);
            DisConnect();
            return true;
        }


        /// <summary>
        /// �����̐ڑ�����������
        /// </summary>
        public void DisConnect()
        {
            // �q�Ɏ����̐ڑ�����������悤�ɓ`����
            foreach (var e in Edges)
                e.RemoveFace(this);

            // �e�Ɏ����̐ڑ�����������悤�ɓ`����
            Graph?.RemoveFace(this);

            edges.Clear();
        }
    }

    [Serializable]
    public class RGraph : ARnParts<RGraph>
    {
        //----------------------------------
        // start: �t�B�[���h
        //----------------------------------
        private HashSet<RFace> faces = new HashSet<RFace>();

        //----------------------------------
        // end: �t�B�[���h
        //----------------------------------
        /// <summary>
        /// ��
        /// </summary>
        public IReadOnlyCollection<RFace> Faces => faces;

        /// <summary>
        /// �SEdge���擾(�d��)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<REdge> GetAllEdges()
        {
            return Faces.SelectMany(p => p.Edges).Distinct();
        }

        /// <summary>
        /// �SVertex���擾(�d��)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RVertex> GetAllVertices()
        {
            return GetAllEdges().SelectMany(e => e.Vertices).Distinct();
        }

        /// <summary>
        /// �eFace�ǉ�
        /// </summary>
        /// <param name="face"></param>
        public void AddFace(RFace face)
        {
            if (face == null)
                return;
            if (faces.Contains(face))
                return;
            faces.Add(face);
            face.SetGraph(this);
        }

        /// <summary>
        /// �eFace�폜
        /// </summary>
        /// <param name="face"></param>
        public void RemoveFace(RFace face)
        {
            faces.Remove(face);
            face?.RemoveGraph(this);
        }
    }

    public class RFaceGroup
    {
        public RGraph Graph { get; }

        public PLATEAUCityObjectGroup CityObjectGroup { get; }

        public HashSet<RFace> Faces { get; } = new HashSet<RFace>();

        public RRoadTypeMask RoadTypes
        {
            get
            {
                return Faces.Aggregate((RRoadTypeMask)0, (a, f) => a | f.RoadTypes);
            }
        }

        public RFaceGroup(RGraph graph, PLATEAUCityObjectGroup cityObjectGroup, IEnumerable<RFace> faces)
        {
            Graph = graph;
            CityObjectGroup = cityObjectGroup;
            foreach (var face in faces)
                Faces.Add(face);
        }
    }

    public static class RVertexEx
    {
        /// <summary>
        /// faceSelector�Ŏw�肵��RFace������RRoadType�𓝍����Ď擾
        /// </summary>
        /// <param name="self"></param>
        /// <param name="faceSelector"></param>
        /// <returns></returns>
        public static RRoadTypeMask GetRoadType(this RVertex self, Func<RFace, bool> faceSelector)
        {
            RRoadTypeMask roadType = RRoadTypeMask.Empty;
            foreach (var face in self.GetFaces().Where(faceSelector))
            {
                roadType |= face.RoadTypes;
            }
            return roadType;
        }
    }
}
