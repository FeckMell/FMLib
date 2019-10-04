using System.Collections.Generic;
using System.Linq;
using Utils.ExtensionMethods;

namespace Utils.Collections
{
  /// <summary>
  /// Collection of elements represented as tree
  /// </summary>
  public class DataTree<T>
  {

    #region Fields

    /// <summary> Used in <see cref="ToString()"/> to separate errors. </summary>
    public const string DELIMETER = "\n";
    /// <summary> Used in <see cref="ToString()"/> to indent levels. </summary>
    public const string INTEND = "\t";

    /// <summary>
    /// Value getter and setter.
    /// <para/>Setter for editing purpose only. E.g. do not use if can use <see cref="DataTree{T}.Add(T)"/> instead
    /// </summary>
    public T Value { get => m_value; set => m_value = value; }

    /// <summary>
    /// Getter for nodes.
    /// <para/>No setter should be used!
    /// </summary>
    public List<DataTree<T>> Nodes { get => m_nodes; }

    /// <summary>
    /// Checks if instance contains no data
    /// </summary>
    public bool IsEmpty => m_value == null && m_nodes.Count == 0;

    /// <summary>
    /// Value in node
    /// </summary>
    private Value<T> m_value;

    /// <summary>
    /// Child nodes
    /// </summary>
    private List<DataTree<T>> m_nodes = new List<DataTree<T>>();

    /// <summary> Id of that instance </summary>
    private readonly long m_id;
    /// <summary> Id counter </summary>
    private static long s_idCounter = 0;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Default constructor
    /// <para/>Tries to fill higher data first.
    /// </summary>
    public DataTree() { }

    /// <summary>
    /// Constructor from value
    /// <para/>Tries to fill higher data first.
    /// </summary>
    public DataTree(T value) : this(value, null) { }

    /// <summary>
    /// Constructor from nodes. Do not use as copy constructor! Use <see cref="Utils.ExtensionMethods.ExtensionsObject.Clone{T}(T)"/>
    /// <para/>Tries to fill higher data first.
    /// </summary>
    public DataTree(DataTree<T> node) : this(null, node) { }

    /// <summary>
    /// Constructor from all data
    /// <para/>Tries to fill higher data first.
    /// </summary>
    public DataTree(T value, DataTree<T> node) : this((Value<T>)value, node) { }

    /// <summary>
    /// Constructor from all data with logicEDS
    /// </summary>
    private DataTree(Value<T> value, DataTree<T> node)
    {
      m_id = s_idCounter++;
      Fill(value, node);
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Adds value to instance.
    /// <para/>Tries to fill higher data first.
    /// </summary>
    public void Add(T value)
    {
      Fill(value, null);
    }

    /// <summary>
    /// Adds node to instance.
    /// <para/>Tries to fill higher data first.
    /// </summary>
    public void Add(DataTree<T> node)
    {
      Fill(null, node);
    }

    /// <summary>
    /// Adds data to instance.
    /// <para/>Tries to fill higher data first.
    /// </summary>
    public void Add(T value, DataTree<T> node)
    {
      Fill(value, node);
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
      return ToString(DELIMETER, INTEND);
    }

    /// <summary>
    /// ToString
    /// </summary>
    public string ToString(string delimeter = DELIMETER, string intend = INTEND)
    {
      return ToString(delimeter, intend, 0, new HashSet<long>());
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Method that contains all logic of filling current instance with provided data.
    /// <para/>MUST be used in constructors or Add methods
    /// </summary>
    private void Fill(Value<T> value, DataTree<T> node)
    {
      // THIS METHOD CONTAINS ALL LOGIC OF FILLING INSTANCE WITH PROVIDED DATA.
      // WANT TO KEEP ALL THAT LOGIC IN ONE METHOD TO AVOID INCORRECT USINGS OF SEPARATE METHODS

      if (m_value == null) // m_value==null
      {
        if (value == null && node == null) // m_value==null, value==null, node==null
        {
          return;
        }

        if (value != null && node == null) // m_value==null, value!=null, node==null
        {
          m_value = value;
          return;
        }

        if (value == null && node != null) // m_value==null, value==null, node!=null
        {
          if (node.m_value == null) // m_value==null, value==null, node!=null, node.m_value==null
          {
            m_nodes.AddRange(node.m_nodes);
            return;
          }
          else // m_value==null, value==null, node!=null, node.m_value!=null
          {
            m_value = node.m_value;
            return;
          }
        }

        if (value != null && node != null) // m_value==null, value!=null, node!=null
        {
          if (node.m_value == null) // m_value==null, value!=null, node!=null, node.m_value==null
          {
            m_value = value;
            m_nodes.AddRange(node.m_nodes);
            return;
          }
          else // m_value==null, value!=null, node!=null, node.m_value!=null
          {
            m_value = value;
            m_nodes.Add(node);
            return;
          }
        }
      }


      else // m_value!=null
      {
        if (value == null && node == null) // m_value!=null, value==null, node==null
        {
          return;
        }

        if (value != null && node == null) // m_value!=null, value!=null, node==null
        {
          m_nodes.Add(new DataTree<T>(value));
          return;
        }

        if (value == null && node != null) // m_value!=null, value==null, node!=null
        {
          if (node.m_value == null) // m_value!=null, value==null, node!=null, node.m_value==null
          {
            m_nodes.AddRange(node.m_nodes);
            return;
          }
          else // m_value!=null, value==null, node!=null, node.m_value!=null
          {
            m_nodes.Add(node);
            return;
          }
        }

        if (value != null && node != null) // m_value!=null, value!=null, node!=null
        {
          if (node.m_value == null) // m_value!=null, value!=null, node!=null, node.m_value==null
          {
            m_nodes.Add(new DataTree<T>(value, node)); // will add node with {m_value=value, m_nodes=node.m_nodes}
            return;
          }
          else // m_value!=null, value!=null, node!=null, node.m_value!=null
          {
            m_nodes.Add(new DataTree<T>(value, node)); // will add node with {m_value=value, m_nodes=[node]}
            return;
          }
        }
      }
    }

    /// <summary>
    /// ToString. Not efficient due to call new HashSet(thatHashSet) for each tree branch.
    /// <para/>Takes 5ms for executing 100 times with 7 branches. Takes 160ms for <see cref="ExtensionMethods.ExtensionsObject.AsString(object)"/> in same situation
    /// </summary>
    private string ToString(string delimeter, string intend, int depth, HashSet<long> loopback)
    {
      if (IsEmpty) { return string.Empty; }

      string rootIntend = intend.Repeat(depth);
      List<string> result = new List<string>();
      if (m_value != null) { result.Add($"{rootIntend}{m_value}(LoopBack id={m_id})"); }

      if (loopback.Add(m_id))
      {
        if (m_nodes.Count > 0) { result.AddRange(m_nodes.Select(x => x.ToString(delimeter, intend, depth + 1, new HashSet<long>(loopback)))); }
      }
      else { result.Add($"{rootIntend + intend}Faced LoopBack id={m_id})"); }
      return result.StrJoin(delimeter);
    }

    #endregion

  }
}