using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Diagnostics;

namespace AssetImporter {
    public class Converter  {

        public static BackgroundWorker worker;

        public void Convert(string sourceDirectory, string targetDirectory, BackgroundWorker backgroundWorker) {
            var jobs = new List<Task>();
            worker = backgroundWorker;

            foreach (var file in fileList) {
                if (isDirectory(sourceDirectory + file)) {
                    foreach (var subFile in Directory.GetFiles(sourceDirectory + file)) {
                        var baseDir = targetDirectory + file;
                        var fileName = file + Path.GetFileName(subFile);
                        var lambdaSubFile = subFile;
                        jobs.Add(Task.Factory.StartNew(() => {
                            worker.ReportProgress(0, "Converting " + Path.GetFileName(lambdaSubFile) + '\n');
                            LoadFile(fileName, lambdaSubFile, baseDir);
                        }));
                    }
                }
                else {
                    var baseDir = targetDirectory + file;
                    baseDir = Path.GetDirectoryName(baseDir);
                    string fileName = sourceDirectory + file;
                    var lambdaFile = file;
                    if (file.EndsWith("PAL5.BBM")) {
                        worker.ReportProgress(0, "Converting " + Path.GetFileName(fileName) + '\n');
                        LoadFile(file, fileName, baseDir);
                    }
                    else {
                        jobs.Add(Task.Factory.StartNew(() => {
                            worker.ReportProgress(0, "Converting " + Path.GetFileName(fileName) + '\n');
                            LoadFile(lambdaFile, fileName, baseDir);
                        }));
                    }
                }
            }

            Task.WaitAll(jobs.ToArray());
            Bmp.SaveOffsets(targetDirectory);
            worker.ReportProgress(0, "All done!");
        }

        void LoadFile(string shortName, string fileName, string targetDirectory) {
            Loader.LoadFile(fileName, targetDirectory);
        }

        bool isDirectory(string fileName) {
            return (File.GetAttributes(fileName) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /*
            * RTTR converts all Settlers 2 gold assets except for the following:
            * - The contents of /DATA/TEXTURES (which look like 256x256 paletted textures presenting gradients)
            * /GFX/PICS/SETUP010.LBM            
            * /GFX/PICS/INSTALL.LBM
            * /GFX/PICS/WORLD.LB
            * /GFX/PICS/WORLDMSK.LBM
            * /GFX/PICS/SETUP899.LBM - /GFX/PICS/SETUP998.LBM
            * /GFX/PICS2/CREDIT00.LBM
            * /GFX/TEXTURES/TEXTUR_0.LBM
            * /GFX/TEXTURES/TEXTUR_3.LBM
            * /DATA/MAP_*_Y.LST
            * /DATA/MAP0*.LST
            * /DATA/MAPSBOBS*.LST
            * /DATA/REMAP.DAT
            */

        /*
            * We convert all that RTTR converts except for:
            * - /WORLDS - because my Settlers 2 gold directory doesn't contain it
            */
        List<string> fileList = new List<string> {
                "/GFX/PALETTE/PAL5.BBM",
                "/DATA/RESOURCE.IDX",
                "/DATA/IO/IO.IDX",
                "/GFX/PICS/SETUP013.LBM",
                "/GFX/PICS/SETUP015.LBM",
                "/GFX/PICS/SETUP666.LBM",        
                "/GFX/PICS/SETUP667.LBM",
                "/GFX/PICS/SETUP801.LBM",
                "/GFX/PICS/SETUP802.LBM",
                "/GFX/PICS/SETUP803.LBM",
                "/GFX/PICS/SETUP804.LBM",
                "/GFX/PICS/SETUP805.LBM",
                "/GFX/PICS/SETUP806.LBM",
                "/GFX/PICS/SETUP810.LBM",
                "/GFX/PICS/SETUP811.LBM",
                "/GFX/PICS/SETUP895.LBM",
                "/GFX/PICS/SETUP896.LBM",
                "/GFX/PICS/SETUP899.LBM",
                "/GFX/PICS/SETUP901.LBM",
                "/GFX/PICS/SETUP990.LBM",
                "/GFX/PICS/SETUP996.LBM",
                "/GFX/PICS/SETUP998.LBM",
                "/GFX/PICS/MISSION/AFRICA.LBM",
                "/GFX/PICS/MISSION/AUSTRA.LBM",
                "/GFX/PICS/MISSION/EUROPE.LBM",
                "/GFX/PICS/MISSION/GREEN.LBM",
                "/GFX/PICS/MISSION/JAPAN.LBM",
                "/GFX/PICS/MISSION/NAMERICA.LBM",
                "/GFX/PICS/MISSION/NASIA.LBM",
                "/GFX/PICS/MISSION/SAMERICA.LBM",
                "/GFX/PICS/MISSION/SASIA.LBM",
                "/GFX/TEXTURES/TEX5.LBM", 
                "/GFX/TEXTURES/TEX6.LBM", 
                "/GFX/TEXTURES/TEX7.LBM",
                "/DATA/MAP_0_Z.LST", 
                "/DATA/MAP_1_Z.LST", 
                "/DATA/MAP_2_Z.LST", 
                "/DATA/CBOB/ROM_BOBS.LST",
                "/DATA/MBOB/AFR_Z.LST", 
                "/DATA/MBOB/JAP_Z.LST", 
                "/DATA/MBOB/ROM_Z.LST", 
                "/DATA/MBOB/VIK_Z.LST", 
                "/DATA/MBOB/WAFR_Z.LST",
                "/DATA/MBOB/WJAP_Z.LST",
                "/DATA/MBOB/WROM_Z.LST",
                "/DATA/MBOB/WVIK_Z.LST",
                "/DATA/MBOB/AFR_ICON.LST",
                "/DATA/MBOB/JAP_ICON.LST",
                "/DATA/MBOB/ROM_ICON.LST",
                "/DATA/MBOB/VIK_ICON.LST",
                "/DATA/BOOT_Z.LST",
                "/DATA/BOBS/BOAT.LST",
                "/DATA/SOUNDDAT/SOUND.LST", 
                "/DATA/MIS0BOBS.LST",
                "/DATA/MIS1BOBS.LST",
                "/DATA/MIS2BOBS.LST",
                "/DATA/MIS3BOBS.LST",
                "/DATA/MIS4BOBS.LST",
                "/DATA/MIS5BOBS.LST",
                "/DATA/MAPS3/",
                "/DATA/MAPS4/",
                "/DATA/MAPS2/",
                "/DATA/MAPS/",
                "/DATA/BOBS/CARRIER.BOB",
                "/DATA/BOBS/CARRIER2.BOB",
                "/DATA/BOBS/JOBS.BOB",
                "/DATA/SOUNDDAT/SNG/",
        };
    }
}