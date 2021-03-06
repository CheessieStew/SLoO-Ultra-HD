//See associated .cpp file for copyright and other info

#ifndef __mdaEPiano__
#define __mdaEPiano__

#include <string.h>

#include "audioeffectx.h"

#define NPARAMS 12       //number of parameters
#define NPROGS   8       //number of programs
#define NOUTS    2       //number of outputs
#define NVOICES 32       //max polyphony
#define SUSTAIN 128
#define SILENCE 0.0001f  //voice choking
#define WAVELEN 422414   //wave data bytes

class mdaEPianoProgram
{
  friend class mdaEPiano;
public:
  mdaEPianoProgram() {}
	~mdaEPianoProgram() {}

private:
  float param[NPARAMS];
  char  name[24];
};


struct VOICE  //voice state
{
  long  delta;  //sample playback
  long  frac;
  long  pos;
  long  end;
  long  loop;
  
  float env;  //envelope
  float dec;

  float f0;   //first-order LPF
  float f1;
  float ff;

  float outl;
  float outr;
  long  note; //remember what note triggered this
};


struct KGRP  //keygroup
{
  long  root;  //MIDI root note
  long  high;  //highest note
  long  pos;
  long  end;
  long  loop;
};

class mdaEPiano : public AudioEffectX
{
public:
	mdaEPiano(audioMasterCallback audioMaster);
	~mdaEPiano();

	virtual void process(float **inputs, float **outputs, VstInt32 sampleframes);
	virtual void processReplacing(float **inputs, float **outputs, VstInt32 sampleframes);
	virtual VstInt32 processEvents(VstEvents* events);

	virtual void setProgram(VstInt32 program);
	virtual void setProgramName(char *name);
	virtual void getProgramName(char *name);
	virtual void setParameter(VstInt32 index, float value);
	virtual float getParameter(VstInt32 index);
	virtual void getParameterLabel(VstInt32 index, char *label);
	virtual void getParameterDisplay(VstInt32 index, char *text);
	virtual void getParameterName(VstInt32 index, char *text);
	virtual void setBlockSize(VstInt32 blockSize);
  virtual void resume();

	virtual bool getOutputProperties (VstInt32 index, VstPinProperties* properties);
	virtual bool getProgramNameIndexed (VstInt32 category, VstInt32 index, char* text);
	virtual bool copyProgram (VstInt32 destination);
	virtual bool getEffectName (char* name);
	virtual bool getVendorString (char* text);
	virtual bool getProductString (char* text);
	virtual VstInt32 getVendorVersion () {return 1;}
	virtual VstInt32 canDo (char* text);

	virtual VstInt32 getNumMidiInputChannels ()  { return 1; }

  long guiUpdate;
  void guiGetDisplay(VstInt32 index, char *label);

private:
	void update();  //my parameter update
  void noteOn(long note, long velocity);
  void fillpatch(long p, char *name, float p0, float p1, float p2, float p3, float p4,
                 float p5, float p6, float p7, float p8, float p9, float p10,float p11);

  float param[NPARAMS];
  mdaEPianoProgram* programs;
  float Fs, iFs;

  #define EVENTBUFFER 120
  #define EVENTS_DONE 99999999
  long notes[EVENTBUFFER + 8];  //list of delta|note|velocity for current block

  ///global internal variables
  KGRP  kgrp[34];
  VOICE voice[NVOICES];
  long  activevoices, poly;
  short *waves;
  float width;
  long  size, sustain;
  float lfo0, lfo1, dlfo, lmod, rmod;
  float treb, tfrq, tl, tr;
  float tune, fine, random, stretch, overdrive;
  float muff, muffvel, sizevel, velsens, volume, modwhl;
};

#endif
