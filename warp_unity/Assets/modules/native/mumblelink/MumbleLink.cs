using System;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Text;
using Newtonsoft.Json;

namespace SuMumbleLinkGW2
{
    public class MumbleLink : IDisposable
    {
        MemoryMappedFile m_mappedFile;
        MemoryMappedViewAccessor m_accessor;

        MumbleLinkedMemory m_mumbleLinkedMemory = new MumbleLinkedMemory();
        GW2Info m_gw2Info = new GW2Info();

        // Constructor
        public MumbleLink()
        {
            m_mappedFile = MemoryMappedFile.CreateOrOpen("MumbleLink", Marshal.SizeOf(m_mumbleLinkedMemory));
            m_accessor = m_mappedFile.CreateViewAccessor(0, Marshal.SizeOf(m_mumbleLinkedMemory));
        }

        public MumbleLinkedMemory mumbleLinkMemoryRead()
        {
            m_accessor.Read(0, out m_mumbleLinkedMemory);
            return m_mumbleLinkedMemory;
        }

        public GW2Info Read()
        {
            m_accessor.Read(0, out m_mumbleLinkedMemory);
            m_gw2Info.initGW2Info(m_mumbleLinkedMemory);
            return m_gw2Info;
        }

        public void Dispose()
        {
            m_mappedFile.Dispose();
        }
    }
}
