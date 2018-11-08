using System;
using System.Collections.Generic;

using System.Windows.Forms;
using System.Runtime.InteropServices;

using OpcRcw.Da;

using Pinvoke;
using Extensions;

namespace YZXLogicEngine.Unit
{
  /// <summary>
  /// SimaticNET模块
  /// </summary>
  public class SimaticNETUnit : YZXWriteableUnit
  {
    public string OPCTag { get; set; }
    /* Constants */
    internal const string SERVER_NAME = "OPC.SimaticNET";       // local server name
    internal const string ITEM_NAME_Prefix = "PNIO:[ctrl2]";          // First item name
    internal const string GROUP_NAME = "grp1";                  // Group name
    internal const int LOCALE_ID = 0x409;                       // LOCALE ID FOR ENGLISH.

    /* Global variables */
    IOPCServer pIOPCServer;   // OPC server interface pointer
    object pobjGroup1;        // Pointer to group object
    int nSvrGroupID;          // server group handle for the added group
    IOPCSyncIO pIOPCSyncIO;   // instance pointer for synchronous IO.
    int[] nItemSvrID;         // array for server handles for

    public SimaticNETUnit(string opctag)
    {
      Name = string.Format("SimaticNET-{0}", opctag);
      OPCTag = opctag;
      Init();
    }

    public override void Init()
    {
      // Local variables
      Type svrComponenttyp;
      OPCITEMDEF[] ItemDefArray;

      // Initialise Group properties
      int bActive = 0;
      int dwRequestedUpdateRate = 250;
      int hClientGroup = 0;
      int dwLCID = LOCALE_ID;
      int pRevUpdateRate;
      int TimeBias = 0;
      float deadband = 0;

      // Access unmanaged COM memory
      GCHandle hTimeBias, hDeadband;

      hTimeBias = GCHandle.Alloc(TimeBias, GCHandleType.Pinned);
      hDeadband = GCHandle.Alloc(deadband, GCHandleType.Pinned);

      // 1. Connect to the local server.
      // Get the Type from the progID and create instance of the OPC Server COM
      // component
      Guid iidRequiredInterface = typeof(IOPCItemMgt).GUID;
      svrComponenttyp = Type.GetTypeFromProgID(SERVER_NAME);
      try
      {
        pIOPCServer = (IOPCServer)Activator.CreateInstance(svrComponenttyp);
        try
        {
          /* 2. Add a new group
              Add a group object and querry for interface IOPCItemMgt
              Parameter as following:
              [in] not active, so no OnDataChange callback
              [in] Request this Update Rate from Server
              [in] Client Handle, not necessary in this sample
              [in] No time interval to system UTC time
              [in] No Deadband, so all data changes are reported
              [in] Server uses english language to for text values
              [out] Server handle to identify this group in later calls
              [out] The answer from Server to the requested Update Rate
              [in] requested interface type of the group object
              [out] pointer to the requested interface
          */
          pIOPCServer.AddGroup(GROUP_NAME,
              bActive,
              dwRequestedUpdateRate,
              hClientGroup,
              hTimeBias.AddrOfPinnedObject(),
              hDeadband.AddrOfPinnedObject(),
              dwLCID,
              out nSvrGroupID,
              out pRevUpdateRate,
              ref iidRequiredInterface,
              out pobjGroup1);

          // Get the IOPOCSyncIO interface pointer from the group pointer.
          // pIOPCSyncIO also needs to be released when not in use.
          pIOPCSyncIO = (IOPCSyncIO)pobjGroup1;

          /* 3. Add items to the group */
          // This example shows adding of only one item in the group

          ItemDefArray = new OPCITEMDEF[1];

          ItemDefArray[0].szAccessPath = "";             // Accesspath not needed for this sample
          ItemDefArray[0].szItemID = ITEM_NAME_Prefix + OPCTag;     // ItemID, see above
          ItemDefArray[0].bActive = 1;              // item is active
          ItemDefArray[0].hClient = 1;              // client handle
          ItemDefArray[0].dwBlobSize = 0;              // blob size
          ItemDefArray[0].pBlob = IntPtr.Zero;    // pointer to blob
          ItemDefArray[0].vtRequestedDataType = 2;              // return values in native (cannonical) datatype

          // initialize output parameters.
          IntPtr pResults = IntPtr.Zero;
          IntPtr pErrors = IntPtr.Zero;
          try
          {
            // Add items to group
            ((IOPCItemMgt)pobjGroup1).AddItems(1, ItemDefArray, out pResults, out pErrors);
            // Unmarshal to get the server handles out fom the m_pItemResult
            // after checking the errors
            int[] errors = new int[1];
            Marshal.Copy(pErrors, errors, 0, 1);
            if (errors[0] == 0)
            {
              OPCITEMRESULT result = (OPCITEMRESULT)Marshal.PtrToStructure(pResults, typeof(OPCITEMRESULT));
              // Allocate integer array
              nItemSvrID = new int[1];
              nItemSvrID[0] = result.hServer;
            }
            else
            {
              string pstrError;
              pIOPCServer.GetErrorString(errors[0], LOCALE_ID, out pstrError);
              MessageBox.Show(pstrError,
                  "Result - Adding Items", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // Free indirect structure elements
            Marshal.DestroyStructure(pResults, typeof(OPCITEMRESULT));
          }
          catch (Exception error) // catch for error in adding items.
          {
            MessageBox.Show(error.Message,
                "Result - Adding Items", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
          finally
          {
            // Free the unmanaged COM memory
            if (pResults != IntPtr.Zero)
            {
              Marshal.FreeCoTaskMem(pResults);
              pResults = IntPtr.Zero;
            }
            if (pErrors != IntPtr.Zero)
            {
              Marshal.FreeCoTaskMem(pErrors);
              pErrors = IntPtr.Zero;
            }
          }
        }
        catch (Exception error) // catch for error in creation group
        {
          MessageBox.Show(string.Format("Error while creating group object:-{0}", error.Message),
              "Result-Add Group", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
          if (hDeadband.IsAllocated) hDeadband.Free();
          if (hTimeBias.IsAllocated) hTimeBias.Free();
        }

      }
      catch (Exception error) // catch for error in creating server
      {
        MessageBox.Show(string.Format("Error while creating server object:-{0}", error.Message),
            "Result - Create Server Instance", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #region YZXUnit
    public override List<bool> UpdateBits()
    {
      // Access unmanaged COM memory
      IntPtr pItemValues = IntPtr.Zero;
      IntPtr pErrors = IntPtr.Zero;
      try
      {
        // Sync read from device
        pIOPCSyncIO.Read(OPCDATASOURCE.OPC_DS_DEVICE, 1, nItemSvrID, out pItemValues, out pErrors);
        // Unmarshal the returned memory to get the item state out fom the ppItemValues
        // after checking errors
        int[] errors = new int[1];
        Marshal.Copy(pErrors, errors, 0, 1);
        if (errors[0] == 0)
        {
          OPCITEMSTATE pItemState = (OPCITEMSTATE)Marshal.PtrToStructure(pItemValues, typeof(OPCITEMSTATE));

          RecodeUpdatetimes();

          ushort b = ushort.Parse(pItemState.vDataValue.ToString());

          bool[] rbits = b.ToBoolArray();
          for (int j = 0; j < 16; j++)
          {
            bits.Add(rbits[j]);
          }

          // Free indirect variant element, other indirect elements are freed by Marshal.DestroyStructure(...)
          oleaut32.VariantClear(pItemValues + 0);

          return bits;
        }
        else
        {
          string pstrError;
          pIOPCServer.GetErrorString(errors[0], LOCALE_ID, out pstrError);
          return new List<bool>();
        }

        // Free indirect structure elements
        Marshal.DestroyStructure(pItemValues, typeof(OPCITEMSTATE));
      }
      catch (Exception error)
      {
        MessageBox.Show(error.Message,
            "Result - Read Items", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        // Free the unmanaged COM memory
        if (pItemValues != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(pItemValues);
          pItemValues = IntPtr.Zero;
        }
        if (pErrors != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(pErrors);
          pErrors = IntPtr.Zero;
        }
      }
      return new List<bool>();
    }

    public override bool WriteB(ushort wIndex, byte bIndex, bool v)
    {
      throw new NotImplementedException();
    }

    public override bool WriteUshort(ushort wIndex, ushort value)
    {
      // Access unmanaged COM memory
      IntPtr pErrors = IntPtr.Zero;

      object[] values = new object[1];
      values[0] = value;

      try
      {
        pIOPCSyncIO.Write(1, nItemSvrID, values, out pErrors);
        int[] errors = new int[1];
        Marshal.Copy(pErrors, errors, 0, 1);

        string pstrError;
        pIOPCServer.GetErrorString(errors[0], LOCALE_ID, out pstrError);
        if(errors[0] == 0)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
      finally
      {
        // Free the unmanaged COM memory
        if (pErrors != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(pErrors);
          pErrors = IntPtr.Zero;
        }
      }
      return false;
    }

    public override bool WriteD(ushort dwIndex, int d)
    {
      throw new NotImplementedException();
    }

    public override bool WriteBits(List<bool> bits)
    {
      throw new NotImplementedException();
    }
    #endregion YZXUnit
  }
}
