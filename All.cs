using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Markup;
using Microsoft.VisualBasic;
using System.Buffers.Binary;
using System.Linq;
using System.Windows.Input;
using System.Text.RegularExpressions;
namespace HashLinkExtractor;
abstract class HLopcode{
	public int exhausted=0;

	public delegate HLopcode Parser(HLbytecode block,ref int value);
  //Not all opcodes are implemented. Notably, Dead Cells does not have any catch opcodes; There is no exception catching. A single bug, and the entire thing crashes. Guess its a good thing the devs know what they're doing...
	public static Parser[] parsers= [HLMovOpcode.parse,HLIntOpcode.parse,HLFloatOpcode.parse,HLBoolOpcode.parse,HLBytesOpcode.parse,
	HLStringOpcode.parse,HLNullOpcode.parse,HLAddOpcode.parse,HLSubOpcode.parse,HLMulOpcode.parse,
	HLSDivOpcode.parse,HLUDivOpcode.parse,HLSModOpcode.parse,HLUModOpcode.parse,HLShlOpcode.parse,
	HLSShrOpcode.parse,HLUShrOpcode.parse,HLAndOpcode.parse,HLOrOpcode.parse,HLXorOpcode.parse,
	HLNegOpcode.parse,HLNotOpcode.parse,HLIncrOpcode.parse,HLDecrOpcode.parse,HLCall0Opcode.parse,
	HLCall1Opcode.parse,HLCall2Opcode.parse,HLCall3Opcode.parse,HLCall4Opcode.parse,HLCallNOpcode.parse,
	HLCallMethodOpcode.parse,HLCallThisOpcode.parse,HLCallClosureOpcode.parse,HLStaticClosureOpcode.parse,HLInstanceClosureOpcode.parse,
	HLVirtualClosureOpcode.parse,HLGetGlobalOpcode.parse,HLSetGlobalOpcode.parse,HLFieldOpcode.parse,HLSetFieldOpcode.parse,HLGetThisOpcode.parse,
	HLSetThisOpcode.parse,HLDynGetOpcode.parse,HLDynSetOpcode.parse,HLJTrueOpcode.parse,HLJFalseOpcode.parse,
	HLJNullOpcode.parse,HLJNotNullOpcode.parse,HLJSLtOpcode.parse,HLJSGteOpcode.parse,HLJSGtOpcode.parse,
	HLJSLteOpcode.parse,HLJULtOpcode.parse,HLJUGteOpcode.parse,/*these two might be named wrong, idk*/HLJUGtOpcode.parse,HLJULteOpcode.parse/**/,
	HLJEqOpcode.parse,HLJNotEqOpcode.parse,HLJAlwaysOpcode.parse,HLToDynOpcode.parse,HLToSFloatOpcode.parse,
	HLToUFloatOpcode.parse,HLToIntOpcode.parse,HLSafeCastOpcode.parse,HLUnsafeCastOpcode.parse,HLToVirtualOpcode.parse,
	HLLabelOpcode.parse,HLRetOpcode.parse,HLThrowOpcode.parse,HLRethrowOpcode.parse,HLSwitchOpcode.parse,
	HLNullCheckOpcode.parse,HLTrapOpcode.parse,HLEndTrapOpcode.parse,HLGetI8Opcode.parse,HLGetI16Opcode.parse,
	HLGetMemOpcode.parse,HLGetArrayOpcode.parse,HLSetI8Opcode.parse,HLSetI16Opcode.parse,HLSetMemOpcode.parse,
	HLSetArrayOpcode.parse,HLNewOpcode.parse,HLArraySizeOpcode.parse,HLTypeOpcode.parse,HLGetTypeOpcode.parse,HLGetTIDOpcode.parse,HLRefOpcode.parse,HLUnrefOpcode.parse,null,HLMakeEnumOpcode.parse,HLEnumAllocOpcode.parse,HLEnumIndexOpcode.parse,HLEnumFieldOpcode.parse,HLSetEnumFieldOpcode.parse,null,HLRefDataOpcode.parse,HLRefOffsetOpcode.parse,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null];
  //for flagging things like for loops, partially implemented
	public enum Flags
	{
		None=(byte)0,
		Break=(byte)1,
		Continue=(byte)2,
		ForLoop=(byte)3
	}
	public unsafe virtual string Apply(DecompRegs regs)
	{
		return "";
	}
	public static HLopcode deserialize(HLbytecode bytes, ref int loc)
	{
		byte b=bytes.getByte(ref loc);
		if(HLopcode.parsers[b]!=null){
			return (HLopcode.parsers[b].Invoke(bytes, ref loc));
		}
		else
		{
			throw new Exception("Unimplemented opcode "+((int)b));
		}
	}
}
abstract class HLopcodeWithDST:HLopcode
{
	public int dst;
}
class HLMovOpcode : HLopcodeWithDST
{
	public int src;
	public HLMovOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"="+regs.registervalues[src];
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[src];
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLMovOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLIntOpcode : HLopcodeWithDST{
	
	public int ptr;
	public HLIntOpcode(int dst,int ptr){
		this.dst=dst;
		this.ptr=ptr;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"=" +Utils.ints[ptr];
		}else{
			regs.registervalues[dst]=Utils.ints[ptr].ToString();
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLIntOpcode(block.getReg(ref loc),block.getIntRef(ref loc));
        
	}
}
class HLFloatOpcode : HLopcodeWithDST{
	
	public int ptr;
	public HLFloatOpcode(int dst,int ptr){
		this.dst=dst;
		this.ptr=ptr;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"=" +Utils.doubles[ptr];
		}else{
			regs.registervalues[dst]=Utils.doubles[ptr].ToString();
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLFloatOpcode(block.getReg(ref loc),block.getIntRef(ref loc));
        
	}
}
class HLBoolOpcode : HLopcodeWithDST{
	
	public bool v;
	public HLBoolOpcode(int dst,bool v){
		this.dst=dst;
		this.v=v;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"=" +(v?"true":"false");
		}else{
			regs.registervalues[dst]=(v?"true":"false");
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLBoolOpcode(block.getReg(ref loc),block.getInlineBool(ref loc));
        
	}
}
class HLBytesOpcode : HLopcodeWithDST{
	
	public int ptr;
	public HLBytesOpcode(int dst,int ptr){
		this.dst=dst;
		this.ptr=ptr;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"=b\"" +string.Concat(Utils.bytes[ptr].Select(b => (char)b < ' ' ? $@"\x{(int)b:x2}" : b.ToString()))+"\"";
		}else{
			regs.registervalues[dst]="b\""+string.Concat(Utils.bytes[ptr].Select(b => (char)b < ' ' ? $@"\x{(int)b:x2}" : b.ToString()))+"\"";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLBytesOpcode(block.getReg(ref loc),block.getRefBytes(ref loc));
        
	}
}
class HLStringOpcode : HLopcodeWithDST{
	
	public int ptr;
	public HLStringOpcode(int dst,int ptr){
		this.dst=dst;
		this.ptr=ptr;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"=\"" +Utils.stable[ptr].Replace("\\","\\\\").Replace("\n","\\n").Replace("\t","\\t").Replace("\'","\\'").Replace("\"","\\\"")+"\"";
		}else{
			regs.registervalues[dst]="\""+Utils.stable[ptr].Replace("\\","\\\\").Replace("\n","\\n").Replace("\t","\\t").Replace("\'","\\'").Replace("\"","\\\"")+"\"";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLStringOpcode(block.getReg(ref loc),block.getStrRef(ref loc));
	}
}
class HLNullOpcode : HLopcodeWithDST{
	
	public HLNullOpcode(int dst){
		this.dst=dst;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"= null";
		}else{
			regs.registervalues[dst]="null";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLNullOpcode(block.getReg(ref loc));
	}
}
class HLAddOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLAddOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return ((r1==r0)?(regs.registervalues[r0]+"+=" +regs.registervalues[r2]):(regs.registervalues[r0]+"="+regs.registervalues[r1]+"+"+regs.registervalues[r2]));
		}else{
			regs.registervalues[r0]=regs.registervalues[r1]+"+"+regs.registervalues[r2];
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLAddOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
        
	}
}
class HLSubOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLSubOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return ((r1==r0)?(regs.registervalues[r0]+"-=" +regs.registervalues[r2]):(regs.registervalues[r0]+"="+regs.registervalues[r1]+"-("+regs.registervalues[r2]+")"));
		}else{
			regs.registervalues[r0]=regs.registervalues[r1]+"-("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLSubOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
        
	}
}
class HLMulOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLMulOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return ((r1==r0)?(regs.registervalues[r0]+"*=" +regs.registervalues[r2]):(regs.registervalues[r0]+"=("+regs.registervalues[r1]+")*("+regs.registervalues[r2]+")"));
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")*("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLMulOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLSDivOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLSDivOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return ((r1==r0)?(regs.registervalues[r0]+"/=" +regs.registervalues[r2]):(regs.registervalues[r0]+"=("+regs.registervalues[r1]+")/("+regs.registervalues[r2]+")"));
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")/("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLSDivOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLUDivOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLUDivOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return ((r1==r0)?(regs.registervalues[r0]+"/=" +regs.registervalues[r2]):(regs.registervalues[r0]+"=("+regs.registervalues[r1]+")/("+regs.registervalues[r2]+")"));
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")/("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLUDivOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLSModOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLSModOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return ((r1==r0)?(regs.registervalues[r0]+"%=" +regs.registervalues[r2]):(regs.registervalues[r0]+"=("+regs.registervalues[r1]+")%("+regs.registervalues[r2]+")"));
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")%("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLSModOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLUModOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLUModOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return ((r1==r0)?(regs.registervalues[r0]+"%=" +regs.registervalues[r2]):(regs.registervalues[r0]+"=("+regs.registervalues[r1]+")%("+regs.registervalues[r2]+")"));
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")%("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLUModOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLShlOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLShlOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return regs.registervalues[r0]+"=("+regs.registervalues[r1]+")<<("+regs.registervalues[r2]+")";
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")<<("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLShlOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLSShrOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLSShrOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return regs.registervalues[r0]+"=("+regs.registervalues[r1]+")>>("+regs.registervalues[r2]+")";
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")>>("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLSShrOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLUShrOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLUShrOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return regs.registervalues[r0]+"=("+regs.registervalues[r1]+")>>>("+regs.registervalues[r2]+")";
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")>>>("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLUShrOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLAndOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLAndOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return regs.registervalues[r0]+"=("+regs.registervalues[r1]+")&("+regs.registervalues[r2]+")";
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")&("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLAndOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLOrOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLOrOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return regs.registervalues[r0]+"=("+regs.registervalues[r1]+")|("+regs.registervalues[r2]+")";
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")|("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLOrOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLXorOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public int  r2;
	public HLXorOpcode(int  r0, int  r1, int  r2){
		this.r0=r0;
		this.r1=r1;
		this.r2=r2;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return regs.registervalues[r0]+"=("+regs.registervalues[r1]+")^("+regs.registervalues[r2]+")";
		}else{
			regs.registervalues[r0]="("+regs.registervalues[r1]+")^("+regs.registervalues[r2]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLXorOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLNegOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public HLNegOpcode(int  r0, int  r1){
		this.r0=r0;
		this.r1=r1;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return regs.registervalues[r0]+"=-("+regs.registervalues[r1]+")";
		}else{
			regs.registervalues[r0]="-("+regs.registervalues[r1]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLNegOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLNotOpcode : HLopcode{
	
	public int  r0;
	public int  r1;
	public HLNotOpcode(int  r0, int  r1){
		this.r0=r0;
		this.r1=r1;
	}
	public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return regs.registervalues[r0]+"=!("+regs.registervalues[r1]+")";
		}else{
			regs.registervalues[r0]="!("+regs.registervalues[r1]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLNotOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLIncrOpcode : HLopcode
{
	public int reg;
	public HLIncrOpcode(int reg)
	{
		this.reg=reg;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(reg)){
			return regs.registervalues[reg]+"++";
		}
		else
		{
			regs.registervalues[reg]=regs.registervalues[reg]+"+1";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLIncrOpcode(block.getReg(ref loc));
	}
}
class HLDecrOpcode : HLopcode
{
	public int reg;
	public HLDecrOpcode(int reg)
	{
		this.reg=reg;
	}
    public unsafe override string Apply(DecompRegs regs){
		return regs.registervalues[reg]+"--";
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLDecrOpcode(block.getReg(ref loc));
	}
}
class HLCall0Opcode : HLopcodeWithDST{
	

	public int fun;
	public HLCall0Opcode(int dst, int fun){
		this.dst=dst;
		this.fun=fun;
	}
	public unsafe override string Apply(DecompRegs regs){
		string fname=Utils.getFIndexName(this.fun);
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+fname+"()";
		}else{
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return fname+"()";
			}
			regs.registervalues[dst]=fname+"()";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCall0Opcode(block.getReg(ref loc),block.getRefFun(ref loc));
	}
}
class HLCall1Opcode : HLopcodeWithDST{
	

	public int fun;
	public int arg0;
	public HLCall1Opcode(int dst, int fun,int arg0){
		this.dst=dst;
		this.fun=fun;
		this.arg0=arg0;
	}
	public unsafe override string Apply(DecompRegs regs){
		string fname=Utils.getFIndexName(this.fun);
		string fstr=Utils.getTrueFuncStringFromNameIndAndArgs(fname,Utils.findex_map[this.fun],[regs.registervalues[arg0]]);
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+fstr;
		}else{
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return fstr;
			}
			regs.registervalues[dst]=fstr;
			
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCall1Opcode(block.getReg(ref loc),block.getRefFun(ref loc),block.getReg(ref loc));
	}
}
class HLCall2Opcode : HLopcodeWithDST{
	

	public int fun;
	public int arg0;
	public int arg1;
	public HLCall2Opcode(int dst, int fun,int arg0,int arg1){
		this.dst=dst;
		this.fun=fun;
		this.arg0=arg0;
		this.arg1=arg1;
	}
	public unsafe override string Apply(DecompRegs regs){
		string fname=Utils.getFIndexName(this.fun);
		string fstr=Utils.getTrueFuncStringFromNameIndAndArgs(fname,Utils.findex_map[this.fun],[regs.registervalues[arg0],regs.registervalues[arg1]]);
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+fstr;
		}else{
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return fstr;
			}
			regs.registervalues[dst]=fstr;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCall2Opcode(block.getReg(ref loc),block.getRefFun(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLCall3Opcode : HLopcodeWithDST{
	

	public int fun;
	public int arg0;
	public int arg1;
	public int arg2;
	public HLCall3Opcode(int dst, int fun,int arg0,int arg1,int arg2){
		this.dst=dst;
		this.fun=fun;
		this.arg0=arg0;
		this.arg1=arg1;
		this.arg2=arg2;
	}
	public unsafe override string Apply(DecompRegs regs){
		string fname=Utils.getFIndexName(this.fun);
		string fstr=Utils.getTrueFuncStringFromNameIndAndArgs(fname,Utils.findex_map[this.fun],[regs.registervalues[arg0],regs.registervalues[arg1],regs.registervalues[arg2]]);
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+fstr;
		}else{
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return fstr;
			}
			regs.registervalues[dst]=fstr;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCall3Opcode(block.getReg(ref loc),block.getRefFun(ref loc),block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLCall4Opcode : HLopcodeWithDST{
	

	public int fun;
	public int arg0;
	public int arg1;
	public int arg2;
	public int arg3;
	public HLCall4Opcode(int dst, int fun,int arg0,int arg1,int arg2,int arg3){
		this.dst=dst;
		this.fun=fun;
		this.arg0=arg0;
		this.arg1=arg1;
		this.arg2=arg2;
		this.arg3=arg3;
	}
	public unsafe override string Apply(DecompRegs regs){
		string fname=Utils.getFIndexName(this.fun);
		string fstr=Utils.getTrueFuncStringFromNameIndAndArgs(fname,Utils.findex_map[this.fun],[regs.registervalues[arg0],regs.registervalues[arg1],regs.registervalues[arg2],regs.registervalues[arg3]]);
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+fstr;
		}else{
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return fstr;
			}
			regs.registervalues[dst]=fstr;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCall4Opcode(block.getReg(ref loc),block.getRefFun(ref loc),block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}

}
class HLCallNOpcode : HLopcodeWithDST{
	

	public int fun;
	public int[] args;
	public HLCallNOpcode(int dst, int fun,int[] args){
		this.dst=dst;
		this.fun=fun;
		this.args=args;
	}
	public unsafe override string Apply(DecompRegs regs){
		string fname=Utils.getFIndexName(this.fun);
		string[] argsstr=new string[args.Length];
		for(int i = 0; i < args.Length; i++)
		{
			argsstr[i]=regs.registervalues[args[i]];
		}
		string fstr=Utils.getTrueFuncStringFromNameIndAndArgs(fname,Utils.findex_map[this.fun],argsstr);
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+fstr;
		}else{
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return fstr;
			}
			regs.registervalues[dst]=fstr;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCallNOpcode(block.getReg(ref loc),block.getRefFun(ref loc),block.getRegs(ref loc));
	}
}
class HLCallMethodOpcode : HLopcodeWithDST{
	

	public int field;
	public int[] args;
	public HLCallMethodOpcode(int dst, int field,int[] args){
		this.dst=dst;
		this.field=field;
		this.args=args;
	}
	public unsafe override string Apply(DecompRegs regs){
		/*
		pindex = df["field"].value
		if op_name == "CallThis":
			recv_t = _reg_tindex(0)
		else:
			args = df["args"].value
			recv_t = _reg_tindex(args[0].value) if args else None
		if recv_t is not None:
			t = code.types[recv_t]
			if isinstance(t.definition, Obj):
				proto = code.proto_by_pindex(t.definition, pindex)
		*/
		Type t=Utils.types[regs.registertypes[this.args[0]]];
		string fname="";
		if(t.definition is ObjType){
			fname =regs.registervalues[this.args[0]]+"."+Utils.stable[Utils.proto_by_pindex((ObjType)t.definition,this.field).Item1];
		}
		else
		{
			fname =regs.registervalues[this.args[0]]+"."+Utils.stable[((VirtualType)t.definition).fields[this.field].Item1];
		}
		if(Utils.isUserDefinedRegistry(dst)){
			string r=regs.registervalues[dst]+"="+fname+"(";
			for(int i = 1; i < args.Length; i++)
			{
				if(i!=1) r+=", ";
				r+=regs.registervalues[args[i]];
			}
			r+=")";
			return r;
		}else{
			string r=fname+"(";
			for(int i = 1; i < args.Length; i++)
			{
				if(i!=1) r+=", ";
				r+=regs.registervalues[args[i]];
			}
			r+=")";
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return r;
			}
			regs.registervalues[dst]=r;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCallMethodOpcode(block.getReg(ref loc),block.getRefField(ref loc),block.getRegs(ref loc));
	}
}
class HLCallThisOpcode : HLopcodeWithDST{
	

	public int field;
	public int[] args;
	public HLCallThisOpcode(int dst, int field,int[] args){
		this.dst=dst;
		this.field=field;
		this.args=args;
	}
	public unsafe override string Apply(DecompRegs regs){
		
		Type t=Utils.types[regs.registertypes[0]];
		string fname ="";
		if(t.definition is ObjType){
			fname ="this"+"."+Utils.stable[Utils.proto_by_pindex((ObjType)t.definition,this.field).Item1];
		}
		else
		{
			fname ="this"+"."+Utils.stable[((VirtualType)t.definition).fields[this.field].Item1];
		}
		if(Utils.isUserDefinedRegistry(dst)){
			string r=regs.registervalues[dst]+"="+fname+"(";
			for(int i = 0; i < args.Length; i++)
			{
				if(i!=0) r+=", ";
				r+=regs.registervalues[args[i]];
			}
			r+=")";
			return r;
		}else{
			string r=fname+"(";
			for(int i = 0; i < args.Length; i++)
			{
				if(i!=0) r+=", ";
				r+=regs.registervalues[args[i]];
			}
			r+=")";
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return r;
			}
			regs.registervalues[dst]=r;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCallThisOpcode(block.getReg(ref loc),block.getRefField(ref loc),block.getRegs(ref loc));
	}
}
class HLCallClosureOpcode : HLopcodeWithDST{
	

	public int fun;
	public int[] args;
	public HLCallClosureOpcode(int dst, int fun,int[] args){
		this.dst=dst;
		this.fun=fun;
		this.args=args;
	}
	public unsafe override string Apply(DecompRegs regs){
		string fname=regs.registervalues[fun];
		if(Utils.isUserDefinedRegistry(dst)){
			string r=regs.registervalues[dst]+"="+fname+"(";
			for(int i = 0; i < args.Length; i++)
			{
				if(i!=0) r+=", ";
				r+=regs.registervalues[args[i]];
			}
			r+=")";
			return r;
		}else{
			string r=fname+"(";
			for(int i = 0; i < args.Length; i++)
			{
				if(i!=0) r+=", ";
				r+=regs.registervalues[args[i]];
			}
			r+=")";
			if(Utils.types[regs.registertypes[dst]].definition is VoidType)
			{
				return r;
			}
			regs.registervalues[dst]=r;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLCallClosureOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getRegs(ref loc));
	}
}
class HLStaticClosureOpcode : HLopcodeWithDST
{

	public int fun;
	public HLStaticClosureOpcode(int dst,int fun)
	{
		this.dst=dst;
		this.fun=fun;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+Utils.GetFNamefromInd(fun);
		}else{
			regs.registervalues[dst]=Utils.GetFNamefromInd(fun);
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLStaticClosureOpcode(block.getReg(ref loc),block.getRefFun(ref loc));
	}
}
class HLInstanceClosureOpcode : HLopcodeWithDST
{

	public int fun;
	public int obj;
	public HLInstanceClosureOpcode(int dst,int fun,int obj)
	{
		this.dst=dst;
		this.fun=fun;
		this.obj=obj;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+regs.registervalues[obj]+"."+Utils.GetFNamefromInd(fun);
		}else{
			regs.registervalues[dst]=regs.registervalues[obj]+"."+Utils.GetFNamefromInd(fun);
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLInstanceClosureOpcode(block.getReg(ref loc),block.getRefFun(ref loc),block.getReg(ref loc));
	}
}
class HLVirtualClosureOpcode : HLopcodeWithDST
{

	public int obj;
	public int field;
	public HLVirtualClosureOpcode(int dst,int obj,int field)
	{
		this.dst=dst;
		this.obj=obj;
		this.field=field;
	}
    public unsafe override string Apply(DecompRegs regs){
		string fname=Utils.stable[((ObjType)Utils.types[Utils._field_map[this.field].Item2].definition).resolve_fields()[Utils._field_map[this.field].Item1].Item1];
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+regs.registervalues[obj]+"."+fname;
		}else{
			regs.registervalues[dst]=regs.registervalues[obj]+"."+fname;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLInstanceClosureOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getRefField(ref loc));
	}
}

class HLGetGlobalOpcode : HLopcodeWithDST
{

	public int global;
	public HLGetGlobalOpcode(int dst,int global)
	{
		this.dst=dst;
		this.global=global;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return (regs.registervalues[dst]+"="+Utils.getGlobalName(global));
		}else{
			regs.registervalues[dst]=Utils.getGlobalName(global);
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLGetGlobalOpcode(block.getReg(ref loc),block.getRefGlobal(ref loc));
	}
}
class HLSetGlobalOpcode : HLopcode
{
	public int global;
	public int src;
	public HLSetGlobalOpcode(int global,int src)
	{
		this.global=global;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		return (HashlinkDecompiler.typeName(Utils.types[Utils.globals[global]]))+"="+regs.registervalues[src];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLSetGlobalOpcode(block.getRefGlobal(ref loc),block.getReg(ref loc));
	}
}
class HLFieldOpcode : HLopcodeWithDST
{

	public int obj;
	public int field;
	public HLFieldOpcode(int dst,int obj,int field)
	{
		this.dst=dst;
		this.obj=obj;
		this.field=field;
	}
    public unsafe override string Apply(DecompRegs regs){
		Type t=Utils.types[regs.registertypes[obj]];
		string fname ="";
		if(t.definition is ObjType){
			fname =Utils.stable[((ObjType)t.definition).resolve_fields()[field].Item1];
		}
		else
		{
			fname =Utils.stable[((VirtualType)t.definition).fields[this.field].Item1];
		}
		if(Utils.isUserDefinedRegistry(dst)){
			return (regs.registervalues[dst]+"="+regs.registervalues[obj]+"."+fname);
		}else{
			regs.registervalues[dst]=regs.registervalues[obj]+"."+fname;
			return "";
		}
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLFieldOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getStrRef(ref loc));
	}
}
class HLSetFieldOpcode : HLopcode
{
	public int src;
	public int field;
	public int obj;
	public HLSetFieldOpcode(int obj,int field,int src)
	{
		this.obj=obj;
		this.src=src;
		this.field=field;
	}
    public unsafe override string Apply(DecompRegs regs){
		return (regs.registervalues[obj]+"."+Utils.getStringTable(field)+"="+regs.registervalues[src]);
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLSetFieldOpcode(block.getReg(ref loc),block.getStrRef(ref loc),block.getReg(ref loc));
	}
}
class HLGetThisOpcode : HLopcodeWithDST
{

	public int field;
	public HLGetThisOpcode(int dst,int field)
	{
		this.dst=dst;
		this.field=field;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return (regs.registervalues[dst]+"=this."+Utils.getStringTable(field));
		}else{
			regs.registervalues[dst]="this."+Utils.getStringTable(field);
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLGetThisOpcode(block.getReg(ref loc),block.getStrRef(ref loc));
	}
}
class HLSetThisOpcode : HLopcode
{
	public int src;
	public int field;
	public HLSetThisOpcode(int field,int src)
	{
		this.src=src;
		this.field=field;
	}
    public unsafe override string Apply(DecompRegs regs){
		
		return ("this."+Utils.getStringTable(((ObjType)Utils.types[regs.registertypes[0]].definition).fields[field].Item1)+"="+regs.registervalues[src]);
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLSetThisOpcode(block.getStrRef(ref loc),block.getReg(ref loc));
	}
}
class HLDynGetOpcode : HLopcodeWithDST
{

	public int obj;
	public int field;
	public HLDynGetOpcode(int dst,int obj,int field)
	{
		this.dst=dst;
		this.obj=obj;
		this.field=field;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return (regs.registervalues[dst]+"="+regs.registervalues[obj]+"["+Utils.getStringTable(field)+"]");
		}else{
			regs.registervalues[dst]=regs.registervalues[obj]+"["+Utils.getStringTable(field)+"]";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLDynGetOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getStrRef(ref loc));
	}
}
class HLDynSetOpcode : HLopcodeWithDST
{

	public int obj;
	public int field;
	public HLDynSetOpcode(int dst,int field,int obj)
	{
		this.dst=dst;
		this.obj=obj;
		this.field=field;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
			return (regs.registervalues[dst]+"["+Utils.getStringTable(field)+"]"+"="+regs.registervalues[obj]);
		}else{
			//throw Exception("Cannot handle DynSet on arbitrary non-variable value");
			return (regs.registervalues[dst]+"["+Utils.getStringTable(field)+"]"+"="+regs.registervalues[obj]);
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLDynSetOpcode(block.getReg(ref loc),block.getStrRef(ref loc),block.getReg(ref loc));
	}
}
abstract class HLJumpOpcode : HLopcode
{
	public int offset;
	public HLJumpOpcode(int offset){
		this.offset=offset;
	}
	public abstract string getCondition(DecompRegs rgs);
	public abstract string getConditionInverted(DecompRegs rgs);
}
class HLJTrueOpcode : HLJumpOpcode
{
	public int reg;
	public HLJTrueOpcode(int reg, int offset) : base(offset)
	{
		this.reg=reg;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return "!"+rgs.registervalues[reg];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJTrueOpcode(block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJFalseOpcode : HLJumpOpcode
{
	public int reg;
	public HLJFalseOpcode(int reg, int offset) : base(offset)
	{
		this.reg=reg;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return "!"+rgs.registervalues[reg];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJFalseOpcode(block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJNullOpcode : HLJumpOpcode
{
	public int reg;
	public HLJNullOpcode(int reg,int offset) : base(offset)
	{
		this.reg=reg;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"==null";
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"!=null";
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJNullOpcode(block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJNotNullOpcode : HLJumpOpcode
{
	public int reg;
	public HLJNotNullOpcode(int reg, int offset) : base(offset)
	{
		this.reg=reg;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"!=null";
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"==null";
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJNotNullOpcode(block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJSLtOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJSLtOpcode(int reg, int reg2,int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"<"+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+">="+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJSLtOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJSGteOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJSGteOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+">="+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"<"+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJSGteOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJSGtOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJSGtOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+">"+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"<="+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJSGtOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJSLteOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJSLteOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"<"+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+">="+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJSLteOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJULtOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJULtOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"<"+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+">="+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJULtOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJUGteOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJUGteOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+">="+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"<"+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJUGteOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJUGtOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJUGtOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+">"+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"<="+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJUGtOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJULteOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJULteOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"<"+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+">="+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJULteOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJEqOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJEqOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"=="+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"!="+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJEqOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJNotEqOpcode : HLJumpOpcode
{
	public int reg;
	public int reg2;
	public HLJNotEqOpcode(int reg, int reg2, int offset) : base(offset)
	{
		this.reg=reg;
		this.reg2=reg2;
	}
	public override string getCondition(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"!="+rgs.registervalues[reg2];
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return rgs.registervalues[reg]+"=="+rgs.registervalues[reg2];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLJNotEqOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLJAlwaysOpcode : HLJumpOpcode
{
	public HLJAlwaysOpcode(int offset) : base(offset)
	{
	}
	public override string getCondition(DecompRegs rgs)
	{
		return "true";
	}
	public override string getConditionInverted(DecompRegs rgs)
	{
		return "false";
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLJAlwaysOpcode(block.getJOffset(ref loc));
	}
}
class HLSafeCastOpcode : HLopcodeWithDST
{

	public int src;

	public HLSafeCastOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if (Utils.isUserDefinedRegistry(dst))
		{
			return regs.registervalues[dst]+" = ("+HashlinkDecompiler.typeName(Utils.types[regs.registertypes[dst]])+")"+regs.registertypes[src];
		}
		else
		{
			regs.registervalues[dst]="("+HashlinkDecompiler.typeName(Utils.types[regs.registertypes[dst]])+")"+regs.registertypes[src];
			return "";
		}
		
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLSafeCastOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLUnsafeCastOpcode : HLopcodeWithDST
{

	public int src;

	public HLUnsafeCastOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if (Utils.isUserDefinedRegistry(dst))
		{
			return regs.registervalues[dst]+" = ("+HashlinkDecompiler.typeName(Utils.types[regs.registertypes[dst]])+")"+regs.registertypes[src];
		}
		else
		{
			regs.registervalues[src]="("+HashlinkDecompiler.typeName(Utils.types[regs.registertypes[dst]])+")"+regs.registertypes[src];
			return "";
		}
		
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLUnsafeCastOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLToVirtualOpcode : HLopcodeWithDST
{

	public int src;
	public HLToVirtualOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"="+regs.registervalues[src];
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[src];
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLToVirtualOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLLabelOpcode : HLopcode
{
    public unsafe override string Apply(DecompRegs regs){
		return "";
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLLabelOpcode();
	}
}
class HLRetOpcode : HLopcode
{
	public int reg;
	public HLRetOpcode(int reg)
	{
		this.reg=reg;
	}
    public unsafe override string Apply(DecompRegs regs){
		return "return "+regs.registervalues[reg];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLRetOpcode(block.getReg(ref loc));
	}
}
class HLThrowOpcode : HLopcode
{
	public int exc;
	public HLThrowOpcode(int exc)
	{
		this.exc=exc;
	}
    public unsafe override string Apply(DecompRegs regs){
		return "throw "+regs.registervalues[exc];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLThrowOpcode(block.getReg(ref loc));
	}
}
class HLRethrowOpcode : HLopcode
{
	public int exc;
	public HLRethrowOpcode(int exc)
	{
		this.exc=exc;
	}
    public unsafe override string Apply(DecompRegs regs){
		return "throw "+regs.registervalues[exc];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLRethrowOpcode(block.getReg(ref loc));
	}
}
class HLSwitchOpcode : HLopcode
{
	public int reg;
	public int[] offsets;
	public int end;
	public HLSwitchOpcode(int reg,int[] offsets,int end)
	{
		this.reg=reg;
		this.offsets=offsets;
		this.end=end;
	}
    public unsafe override string Apply(DecompRegs regs){
		return "";
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLSwitchOpcode(block.getReg(ref loc),block.getJOffsets(ref loc),block.getJOffset(ref loc));
	}
}
class HLNullCheckOpcode : HLopcode
{
	public int reg;
	public HLNullCheckOpcode(int reg)
	{
		this.reg=reg;
	}
    public unsafe override string Apply(DecompRegs regs){
		return "";//"if ("+regs.registervalues[reg]+" != null) throw \"Value null!\"";
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLNullCheckOpcode(block.getReg(ref loc));
	}
}
class HLTrapOpcode : HLopcode
{
	public int exc;
	public int joffset;
	public HLTrapOpcode(int exc,int joffset)
	{
		this.exc=exc;
		this.joffset=joffset;
	}
    public unsafe override string Apply(DecompRegs regs){
		return "";
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLTrapOpcode(block.getReg(ref loc),block.getJOffset(ref loc));
	}
}
class HLEndTrapOpcode : HLopcode
{
	public int exc;
	public HLEndTrapOpcode(int exc)
	{
		this.exc=exc;
	}
    public unsafe override string Apply(DecompRegs regs){
		return "";
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLEndTrapOpcode(block.getReg(ref loc));
	}
}
class HLGetI8Opcode : HLopcodeWithDST
{

	public int bytes;
	public int index;
	public HLGetI8Opcode(int dst,int bytes,int index)
	{
		this.dst=dst;
		this.bytes=bytes;
		this.index=index;
	}
    public unsafe override string Apply(DecompRegs regs){
		if (Utils.isUserDefinedRegistry(dst))
		{
			return regs.registervalues[dst]+" = "+regs.registervalues[bytes]+"["+regs.registervalues[index]+"]";
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[bytes]+"["+regs.registervalues[index]+"]";
			return "";
		}
		
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLGetI8Opcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLGetI16Opcode : HLopcodeWithDST
{

	public int bytes;
	public int index;
	public HLGetI16Opcode(int dst,int bytes,int index)
	{
		this.dst=dst;
		this.bytes=bytes;
		this.index=index;
	}
    public unsafe override string Apply(DecompRegs regs){
		if (Utils.isUserDefinedRegistry(dst))
		{
			return regs.registervalues[dst]+" = "+regs.registervalues[bytes]+"["+regs.registervalues[index]+"]";
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[bytes]+"["+regs.registervalues[index]+"]";
			return "";
		}
		
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLGetI16Opcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLGetMemOpcode : HLopcodeWithDST
{

	public int bytes;
	public int index;
	public HLGetMemOpcode(int dst,int bytes,int index)
	{
		this.dst=dst;
		this.bytes=bytes;
		this.index=index;
	}
    public unsafe override string Apply(DecompRegs regs){
		if (Utils.isUserDefinedRegistry(dst))
		{
			return regs.registervalues[dst]+" = "+regs.registervalues[bytes]+"["+regs.registervalues[index]+"]";
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[bytes]+"["+regs.registervalues[index]+"]";
			return "";
		}
		
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLGetMemOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLGetArrayOpcode : HLopcodeWithDST
{

	public int array;
	public int index;
	public HLGetArrayOpcode(int dst,int array,int index)
	{
		this.dst=dst;
		this.array=array;
		this.index=index;
	}
    public unsafe override string Apply(DecompRegs regs){
		if (Utils.isUserDefinedRegistry(dst))
		{
			return regs.registervalues[dst]+" = "+regs.registervalues[array]+"["+regs.registervalues[index]+"]";
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[array]+"["+regs.registervalues[index]+"]";
			return "";
		}
		
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLGetArrayOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLSetI8Opcode : HLopcode
{
	public int bytes;
	public int index;
	public int src;
	public HLSetI8Opcode(int bytes,int index,int src)
	{
		this.bytes=bytes;
		this.index=index;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
			return regs.registervalues[bytes]+"["+regs.registervalues[index]+"] = "+regs.registervalues[src];		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLSetI8Opcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLSetI16Opcode : HLopcode
{
	public int bytes;
	public int index;
	public int src;
	public HLSetI16Opcode(int bytes,int index,int src)
	{
		this.bytes=bytes;
		this.index=index;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
			return regs.registervalues[bytes]+"["+regs.registervalues[index]+"] = "+regs.registervalues[src];		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLSetI16Opcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLSetMemOpcode : HLopcode
{
	public int bytes;
	public int index;
	public int src;
	public HLSetMemOpcode(int bytes,int index,int src)
	{
		this.bytes=bytes;
		this.index=index;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
			return regs.registervalues[bytes]+"["+regs.registervalues[index]+"] = "+regs.registervalues[src];		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLSetMemOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLSetArrayOpcode : HLopcode
{
	public int array;
	public int index;
	public int src;
	public HLSetArrayOpcode(int array,int index,int src)
	{
		this.array=array;
		this.index=index;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
			return regs.registervalues[array]+"["+regs.registervalues[index]+"] = "+regs.registervalues[src];		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLSetArrayOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLNewOpcode : HLopcode
{
	public int reg;
	public HLNewOpcode(int reg)
	{
		this.reg=reg;
	}
    public unsafe override string Apply(DecompRegs regs){
		if (Utils.isUserDefinedRegistry(reg))
		{
			return regs.registervalues[reg]+" = new "+HashlinkDecompiler.typeName(Utils.types[regs.registertypes[reg]])+"()";
		}
		else
		{
			regs.registervalues[reg]="new "+HashlinkDecompiler.typeName(Utils.types[regs.registertypes[reg]])+"()";
			return "";
		}
		
		
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLNewOpcode(block.getReg(ref loc));
	}
}
class HLArraySizeOpcode : HLopcodeWithDST
{

	public int array;
	public HLArraySizeOpcode(int dst,int array)
	{
		this.dst=dst;
		this.array=array;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"=len("+regs.registervalues[array]+")";
		}
		else
		{
			regs.registervalues[dst]="len("+regs.registervalues[array]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLArraySizeOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLTypeOpcode : HLopcodeWithDST
{

	public int ty;
	public HLTypeOpcode(int dst,int ty)
	{
		this.dst=dst;
		this.ty=ty;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"= "+HashlinkDecompiler.typeName(Utils.types[ty]);
		}
		else
		{
			regs.registervalues[dst]=HashlinkDecompiler.typeName(Utils.types[ty]);
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLTypeOpcode(block.getReg(ref loc),block.getRefType(ref loc));
	}
}
class HLGetTypeOpcode : HLopcodeWithDST
{

	public int src;
	public HLGetTypeOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"=typeof "+regs.registervalues[src];
		}
		else
		{
			regs.registervalues[dst]="typeof "+regs.registervalues[src];
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLGetTypeOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLGetTIDOpcode : HLopcodeWithDST
{

	public int src;
	public HLGetTIDOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"=typeof "+regs.registervalues[src];
		}
		else
		{
			regs.registervalues[dst]="typeof "+regs.registervalues[src];
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLGetTIDOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLRefOpcode : HLopcodeWithDST
{

	public int src;
	public HLRefOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"= &"+regs.registervalues[src];
		}
		else
		{
			regs.registervalues[dst]="&"+regs.registervalues[src];
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLRefOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLUnrefOpcode : HLopcodeWithDST
{

	public int src;
	public HLUnrefOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"= *"+regs.registervalues[src];
		}
		else
		{
			regs.registervalues[dst]="*"+regs.registervalues[src];
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLUnrefOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLMakeEnumOpcode : HLopcodeWithDST
{

	public int construct;
	int[] args;
	public HLMakeEnumOpcode(int dst,int construct,int[] args)
	{
		this.dst=dst;
		this.construct=construct;
		this.args=args;
	}
    public unsafe override string Apply(DecompRegs regs){
		string name=Utils.stable[((EnumType)Utils.types[regs.registertypes[dst]].definition).name]+"."+Utils.stable[((EnumType)Utils.types[regs.registertypes[dst]].definition).constructs[construct].name];
		string[] stringargs=new string[args.Length];
		for(int i = 0; i < args.Length; i++)
		{
			stringargs[i]=regs.registervalues[i];
		}
		string argS=string.Join<string>(", ",stringargs);
		
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+name+"("+argS+")";
		}
		else
		{
			regs.registervalues[dst]=name+"("+argS+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLMakeEnumOpcode(block.getReg(ref loc),block.getRefEnumConstruct(ref loc),block.getRegs(ref loc));
	}
}
class HLEnumAllocOpcode : HLopcodeWithDST
{

	public int construct;
	public HLEnumAllocOpcode(int dst,int construct)
	{
		this.dst=dst;
		this.construct=construct;
	}
    public unsafe override string Apply(DecompRegs regs){
		string name=Utils.stable[((EnumType)Utils.types[regs.registertypes[dst]].definition).name]+"."+Utils.stable[((EnumType)Utils.types[regs.registertypes[dst]].definition).constructs[construct].name];
		
		if(Utils.isUserDefinedRegistry(dst)){
			return regs.registervalues[dst]+"="+name+"()";
		}
		else
		{
			regs.registervalues[dst]=name+"()";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLEnumAllocOpcode(block.getReg(ref loc),block.getRefEnumConstruct(ref loc));
	}
}
class HLEnumIndexOpcode : HLopcodeWithDST
{

	public int value;
	public HLEnumIndexOpcode(int dst,int value)
	{
		this.dst=dst;
		this.value=value;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"= Type.enumIndex("+regs.registervalues[value]+")";
		}
		else
		{
			regs.lastreferencedtypes[dst]=regs.registertypes[value];
			regs.registervalues[dst]="Type.enumIndex("+regs.registervalues[value]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLEnumIndexOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLEnumFieldOpcode : HLopcodeWithDST
{

	public int value;
	public int construct;
	public int field;
	public HLEnumFieldOpcode(int dst,int value,int construct,int field)
	{
		this.dst=dst;
		this.value=value;
		this.construct=construct;
		this.field=field;
	}
    public unsafe override string Apply(DecompRegs regs){
		string fieldname=""+"abcdefghijklmnopqrstuvwxyz"[field];//((EnumType)Utils.types[regs.registertypes[value]]).constructs[construct]._params[field]
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"="+regs.registervalues[value]+"."+fieldname;
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[value]+"."+fieldname;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLEnumFieldOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getRefEnumConstruct(ref loc),block.getRefField(ref loc));
	}
}
class HLSetEnumFieldOpcode : HLopcode
{
	public int value;
	public int field;
	public int src;
	public HLSetEnumFieldOpcode(int value,int field,int src)
	{
		this.value=value;
		this.field=field;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		string fieldname=""+"abcdefghijklmnopqrstuvwxyz"[field];//((EnumType)Utils.types[regs.registertypes[value]]).constructs[construct]._params[field]
		return regs.registervalues[value]+"."+fieldname+"="+regs.registervalues[src];
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLSetEnumFieldOpcode(block.getReg(ref loc),block.getRefField(ref loc),block.getReg(ref loc));
	}
}
class HLRefDataOpcode : HLopcodeWithDST
{

	public int src;
	public HLRefDataOpcode(int dst,int src)
	{
		this.dst=dst;
		this.src=src;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"="+regs.registervalues[src]+".getRef()";
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[src]+".getRef()";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLRefDataOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLRefOffsetOpcode : HLopcodeWithDST
{
	//finish later
	public int reg;
	public int offset;
	public HLRefOffsetOpcode(int dst,int reg,int offset)
	{
		this.dst=dst;
		this.reg=reg;
		this.offset=offset;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(dst)){
		return regs.registervalues[dst]+"="+regs.registervalues[reg]+".offset("+regs.registervalues[offset]+")";
		}
		else
		{
			regs.registervalues[dst]=regs.registervalues[reg]+".offset("+regs.registervalues[offset]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLRefOffsetOpcode(block.getReg(ref loc),block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLToDynOpcode : HLopcode
{
	public int  r0;
	public int  r1;
	public HLToDynOpcode(int  r0, int  r1)
	{
		this.r0=r0;
		this.r1=r1;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return (regs.registervalues[r0]+"="/*(dyn)("*/+regs.registervalues[r1]/*+")"*/);
		}else{
			regs.registervalues[r0]=/*"(dyn)("+*/regs.registervalues[r1]/*+")"*/;
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLToDynOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLToSFloatOpcode : HLopcode
{
	public int  r0;
	public int  r1;
	public HLToSFloatOpcode(int  r0, int  r1)
	{
		this.r0=r0;
		this.r1=r1;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return (regs.registervalues[r0]+"=(float)("+regs.registervalues[r1]+")");
		}else{
			regs.registervalues[r0]="(float)("+regs.registervalues[r1]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		
		return new HLToSFloatOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLToUFloatOpcode : HLopcode
{
	public int  r0;
	public int  r1;
	public HLToUFloatOpcode(int  r0, int  r1)
	{
		this.r0=r0;
		this.r1=r1;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return (regs.registervalues[r0]+"=(float)("+regs.registervalues[r1]+")");
		}else{
			regs.registervalues[r0]="(float)("+regs.registervalues[r1]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLToUFloatOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLToIntOpcode : HLopcode
{
	public int r0;
	public int  r1;
	public HLToIntOpcode(int  r0, int  r1)
	{
		this.r0=r0;
		this.r1=r1;
	}
    public unsafe override string Apply(DecompRegs regs){
		if(Utils.isUserDefinedRegistry(r0)){
			return (regs.registervalues[r0]+"=(int)("+regs.registervalues[r1]+")");
		}else{
			regs.registervalues[r0]="(int)("+regs.registervalues[r1]+")";
			return "";
		}
	}
	public static HLopcode parse(HLbytecode block,ref int loc)
	{
		return new HLToIntOpcode(block.getReg(ref loc),block.getReg(ref loc));
	}
}
class HLbytecode{
	public byte[] bytes;
    public byte get(int loc){
		return bytes[loc];
	}
	public byte getByte(ref int loc){
		loc+=1;
		return bytes[loc-1];
	}
	public static byte getByte(ref int loc,byte[] bytes){
		loc+=1;
		return bytes[loc-1];
	}
	public int getVarInt(ref int loc)
	{
		byte b = getByte(ref loc);
        if(b < 0x80){
            return (int)b;
		}
        if(b < 0xC0){
            byte second = getByte(ref loc);
            int v = ((b & 0x1F) << 8) | second;
            return (v ^ -((b >> 5) & 1)) + ((b >> 5) & 1);
		}
        byte[] remaining_bytes = [getByte(ref loc),getByte(ref loc),getByte(ref loc)];
        //if len(remaining_bytes) < 3:
        //    raise ValueError("Incomplete VarInt at end of stream")
        int remaining = (int)remaining_bytes[2] | 
              ((int)remaining_bytes[1] << 8) | 
              ((int)remaining_bytes[0] << 16);
        int val = ((b & 0x1F) << 24) | remaining;
        return (val ^ -((b >> 5) & 1)) + ((b >> 5) & 1);
	}
	public static int getVarInt(ref int loc, byte[] bytes)
	{
		byte b = getByte(ref loc,bytes);
        if(b < 0x80){
            return (int)b;
		}
        if(b < 0xC0){
            byte second = getByte(ref loc,bytes);
            int v = ((b & 0x1F) << 8) | second;
            return (v ^ -((b >> 5) & 1)) + ((b >> 5) & 1);
		}
        byte[] remaining_bytes = [getByte(ref loc,bytes),getByte(ref loc,bytes),getByte(ref loc,bytes)];
        //if len(remaining_bytes) < 3:
        //    raise ValueError("Incomplete VarInt at end of stream")
        int remaining = (int)remaining_bytes[2] | 
              ((int)remaining_bytes[1] << 8) | 
              ((int)remaining_bytes[0] << 16);
        int val = ((b & 0x1F) << 24) | remaining;
        return (val ^ -((b >> 5) & 1)) + ((b >> 5) & 1);
	}
	
	public int getReg(ref int loc){
		return getVarInt(ref loc);
	}
	public int[] getRegs(ref int loc){
		int[] x= new int[getVarInt(ref loc)];
		for(int i = 0; i < x.Length; i++)
		{
			x[i]=getReg(ref loc);
		}
		return x;
	}
	public int getIntRef(ref int loc){
		return getVarInt(ref loc);
	}
	public int getFloatRef(ref int loc){
		return getVarInt(ref loc);
	}
	public bool getInlineBool(ref int loc){
		return 0!=getVarInt(ref loc);
	}
	public int getRefBytes(ref int loc){
		return getVarInt(ref loc);
	}
	public int getStrRef(ref int loc){
		return getVarInt(ref loc);
	}
	public int getRefFun(ref int loc){
		return getVarInt(ref loc);
	}
	public int getRefField(ref int loc){
		return getVarInt(ref loc);
	}
	public int getRefGlobal(ref int loc){
		return getVarInt(ref loc);
	}
	public int getJOffset(ref int loc){
		return getVarInt(ref loc);
	}
	public int[] getJOffsets(ref int loc){
		return getVarInts(ref loc);
	}
	public int[] getVarInts(ref int loc){
		int[] x= new int[getVarInt(ref loc)];
		for(int i = 0; i < x.Length; i++)
		{
			x[i]=getReg(ref loc);
		}
		return x;
	}
	public int getRefType(ref int loc){
		return getVarInt(ref loc);
	}
	public int getRefEnumConstant(ref int loc){
		return getVarInt(ref loc);
	}
	public int getRefEnumConstruct(ref int loc){
		return getVarInt(ref loc);
	}
	public int getInlineInt(ref int loc){
		return getVarInt(ref loc);
	}
	public int getSInt(int loc){
		return getVarInt(ref loc);
	}

}
class HLopcodes{
	public HLopcode[] opcodes;
	public static HLopcodes fromBytes(HLbytecode bytes)
	{
		int loc=0;
		List<HLopcode> opcodes=new List<HLopcode>();
		while (loc < bytes.bytes.Length)
		{
			byte b=bytes.getByte(ref loc);
			if(HLopcode.parsers[b]!=null){
				opcodes.Add(HLopcode.parsers[b].Invoke(bytes, ref loc));
			}
			else
			{
				throw new Exception("Unimplemented opcode "+((int)b));
			}
		}
		return new HLopcodes(){opcodes=opcodes.ToArray()};
	}
}
class HLblock{
	public unsafe HLbytecode* bytecode;
	public int start;
	public int end;
}

class DecompRegs{
	public string[] registervalues;
	public int[] registertypes;
	public int[] lastreferencedtypes;
	public DecompRegs(int numregvalues){
		registervalues=new string[numregvalues];
		registertypes=new int[numregvalues];
		lastreferencedtypes=new int[numregvalues];
	}
}
class fileRef
{
	public int fid=0;
	public int line=-1;
}
class DebugInfo
{
    /*
    Represents debug information for a function, encoded with a delta encoding scheme for compression.
    */

    public fileRef[] value;

    public static DebugInfo deserialise(byte[] f, int nops, ref int loc){
        List<fileRef> tmp=new List<fileRef>();
        int currfile=-1;
        int currline=0;
        int i = 0;
        while(i < nops){
            try{
				if (f.Length==loc)
					break;
                byte c_byte = f[loc];loc=loc+1;
                int c = c_byte;
                if ((c & 1) != 0){
                    c >>= 1;
					if (f.Length==loc)
                        break;
                    byte b2_byte = f[loc];loc=loc+1;
                    currfile = (c << 8) | ((int)b2_byte);
				}else if ((c & 2) != 0){
                    int delta = c >> 6;
                    int count = (c >> 2) & 15;
                    for(int _=0;_<count;_++){
                        tmp.Add(new fileRef(){fid=currfile, line=currline});
                        i += 1;
					}currline += delta;
				}else if ((c & 4) != 0){
                    currline += c >> 3;
                    tmp.Add(new fileRef(){fid=currfile, line=currline});
                    i += 1;
				}else{
					if (f.Length==loc)
                        break;
                    byte b2_byte=f[loc];loc=loc+1;
					if (f.Length==loc)
                        break;
					byte b3_byte=f[loc];loc=loc+1;
                    int b2 = b2_byte;
                    int b3 = b3_byte;
                    currline = (c >> 3) | (b2 << 5) | (b3 << 13);
                    tmp.Add(new fileRef(){fid=currfile, line=currline});
                    i += 1;
				}
			}
            catch(IndexOutOfRangeException){
				Console.WriteLine("eror");
                break;
			}
		}
        return new DebugInfo(){value=tmp.ToArray()};
	}
}
class LiftedOps
{
	public HLopcode[] ops;
	public LiftedOps[] blocks;
	public static int laststart=-1;
	public static int lastend=-1;
	public static LiftedOps fromOpcodeList(HLopcode[] ops,int start,int end, int loopstart=-1,int loopend=-1)
	{
		//Console.WriteLine("lift");
		if ((start == laststart) && (end == lastend))
		{
			foreach(HLopcode op in ops)
			{
				Console.WriteLine(op);
			}
			Console.WriteLine(start+"-"+end);
			throw new Exception("Infinite loop detected!");
		}
		laststart=start;
		lastend=end;
		List<HLopcode> opcodes=new List<HLopcode>();
		List<LiftedOps> subblocks=new List<LiftedOps>();
		int i=start;
		//Console.WriteLine(start+"-"+end);
		while ((i != end)&&(i<ops.Length)&&(i>=0))
		{
			//Console.WriteLine(i+":");
			//Console.WriteLine("   "+ops[i]);
			int y=i;
			//Console.WriteLine("--"+i);
			//if(ops[i].exhausted){}
			if (ops[i].exhausted > 50)
			{
				throw new Exception("Infinite loop detected!");
			}
			if((ops[i] is HLJumpOpcode potentbreakOpcode)/*&&((jumpOpcode.offset+i)<=end)*/&&((potentbreakOpcode.offset+i)==loopend))
			{
				opcodes.Add(null);
				subblocks.Add(new LiftedOpsIfInverted(){ops=[null],blocks=[new LiftedOpsBreak()],condition=potentbreakOpcode});
			}
			else if((ops[i] is HLJumpOpcode potentcontOpcode)/*&&((jumpOpcode.offset+i)<=end)*/&&((potentcontOpcode.offset+i)==loopstart))
			{
				opcodes.Add(null);
				subblocks.Add(new LiftedOpsIfInverted(){ops=[null],blocks=[new LiftedOpsContinue()],condition=potentcontOpcode});
			}
			else if((ops[i] is HLJumpOpcode jumpOpcode)/*&&((jumpOpcode.offset+i)<=end)*/&&(jumpOpcode.offset>0))
			{
				if(ops[i] is HLJAlwaysOpcode)
				{
					i+=jumpOpcode.offset;
				}	
				else if (((i + jumpOpcode.offset)<ops.Length)&&(ops[i + jumpOpcode.offset] is HLJAlwaysOpcode j2)&&(j2.offset>0))
				{
					
					opcodes.Add(null);
					LiftedOps sb1=fromOpcodeList(ops,i+1,i + jumpOpcode.offset-1,loopstart,loopend);
					LiftedOps sb2=fromOpcodeList(ops,i + jumpOpcode.offset,i + jumpOpcode.offset-1+j2.offset,loopstart,loopend);
					sb1=new LiftedOpsIfElse(){ops=sb1.ops,blocks=sb1.blocks,condition=jumpOpcode,_else=sb2};
					subblocks.Add(sb1);
					i+=jumpOpcode.offset+j2.offset;
					if ((end > y) && (end <= i))
					{
						break;
					}
				}
				else
				{
					int z=i+jumpOpcode.offset;
					opcodes.Add(null);
					if((end > i) && (end < z)){
						LiftedOps sb1=fromOpcodeList(ops,i + jumpOpcode.offset+1,ops.Length,loopstart,loopend);
						sb1=new LiftedOpsIfInverted(){ops=sb1.ops,blocks=sb1.blocks,condition=jumpOpcode};
						subblocks.Add(sb1);
						
					}
					else
					{
						LiftedOps sb1=fromOpcodeList(ops,i+1,i + jumpOpcode.offset+1,loopstart,loopend);
						sb1=new LiftedOpsIf(){ops=sb1.ops,blocks=sb1.blocks,condition=jumpOpcode};
						subblocks.Add(sb1);
						i+=jumpOpcode.offset;
						if ((end > y) && (end <= i))
						{
							break;
						}
					}
					
				}
				
			}else if(ops[i] is HLLabelOpcode labelOpcode)
			{
				int loopEnd=-1;
				for(int j = i; j < end; j++)
				{
					if(j<ops.Length){
						if((ops[j] is HLJumpOpcode jumpop)&&((jumpop.offset+j+1)==i))
						{
							loopEnd=j;
						}
					}
				}
				if(loopEnd!=-1){
					opcodes.Add(null);
					LiftedOps sb1=fromOpcodeList(ops,i+1,loopEnd,i,loopEnd);
					sb1=new LiftedOpsDoWhile(){ops=sb1.ops,blocks=sb1.blocks,condition=(HLJumpOpcode)ops[loopEnd]};
					subblocks.Add(sb1);
					i=loopEnd;
				}
			}else if(ops[i] is HLSwitchOpcode switchOpcode)
			{
				
				int loopEnd=-1;
				List<int> jumpLocs=new List<int>();
				foreach(int off in switchOpcode.offsets)
				{
					if (!jumpLocs.Contains(off))
					{
						jumpLocs.Add(off);
					}
					
				}
				jumpLocs.Sort();
				if (jumpLocs.Contains(0))
				{
					jumpLocs.Remove(0);
				}
				Tuple<int,int> _default=null;
				Tuple<int,int>[] targets=new Tuple<int,int>[jumpLocs.Count];
				if (jumpLocs.Count != 0)
				{
					if (jumpLocs[0] != 1)
					{
						_default=new Tuple<int, int>(1,switchOpcode.end+1);//jumpLocs[0]+1);
						/*if(((i+_default.Item2-1)<ops.Length)&&(ops[i+_default.Item2-1] is HLJAlwaysOpcode jop))
						{
							if ((jop.offset + _default.Item2) == switchOpcode.end)
							{
								_default=new Tuple<int, int>(_default.Item1,_default.Item2-1);
								if ((_default.Item1) == (_default.Item2 - 1))
								{
									_default=null;
								}
							}
						}*/
					}
				}
				//jumpLocs.Add(switchOpcode.end-1);
				for(int k = 0; k < jumpLocs.Count/* - 1*/; k++)
				{
					targets[k]=new Tuple<int, int>(jumpLocs[k]+1,switchOpcode.end+1);//jumpLocs[k+1]+1);
					/*if(((i+targets[k].Item2-1)<ops.Length)&&(ops[i+targets[k].Item2-1] is HLJAlwaysOpcode jop))
					{
						if ((jop.offset + targets[k].Item2) == switchOpcode.end)
						{
							targets[k]=new Tuple<int, int>(targets[k].Item1,targets[k].Item2-1);
						}
					}*/
				}
				LiftedOps[] liftedblocks= new LiftedOps[targets.Length];
				for(int k = 0; k < liftedblocks.Length; k++)
				{
					liftedblocks[k]=fromOpcodeList(ops,i+targets[k].Item1,i+targets[k].Item2,loopstart,loopend);
				}
				LiftedOps _defaultblock=null;
				if (_default != null)
				{
					_defaultblock=fromOpcodeList(ops,i+_default.Item1,i+_default.Item2,loopstart,loopend);
				}
				int[] switchtargets=new int[switchOpcode.offsets.Length];
				for(int k = 0; k < switchOpcode.offsets.Length; k++)
				{
					switchtargets[k]=jumpLocs.IndexOf(switchOpcode.offsets[k]);
				}
				opcodes.Add(null);
				subblocks.Add(new LiftedOpsSwitch(){ops=new HLopcode[liftedblocks.Length],blocks=liftedblocks,_default=_defaultblock,switchReg=switchOpcode.reg,inds=switchtargets});
				i+=switchOpcode.end;
				
			}
			else
			{
				opcodes.Add(ops[i]);
				subblocks.Add(null);
				if(ops[i] is HLThrowOpcode)
				{
					break;
				}
				if(ops[i] is HLRethrowOpcode)
				{
					break;
				}
				if(ops[i] is HLRetOpcode)
				{
					break;
				}
			}
			ops[y].exhausted++;
			i++;
		}
		//Console.WriteLine("end");
		return new LiftedOps(){ops=opcodes.ToArray(),blocks=subblocks.ToArray()};
	}
	public virtual string Parse(DecompRegs regs)
	{
		string x="";
		for(int i = 0; i < ops.Length; i++)
		{
			if (ops[i] != null)
			{
				string o=ops[i].Apply(regs);
				if (o.Length != 0)
				{
					x+="\n\t"+o+";";
				}
				/*else
				{
					x+="\n\t"+ops[i]+";;";
				}*/
			}
			else
			{
				string n=blocks[i].Parse(regs);
				x+="\n\t"+n.Replace("\n","\n\t");
			}
		}
		return x;
	}
	
}
class LiftedOpsSwitch:LiftedOps
{
	public int switchReg;
	public int[] inds;
	public LiftedOps? _default;
	public override string Parse(DecompRegs regs)
	{
		List<int>[] sinds=new List<int>[blocks.Length];
		for(int i = 0; i < sinds.Length; i++)
		{sinds[i]=new List<int>();}
		for(int i = 0; i < inds.Length; i++)
		{
			if((inds[i]>=0)&&(inds[i]<sinds.Length))
				sinds[inds[i]].Add(i);
		}
		string x="switch("+regs.registervalues[switchReg]+"){\n\t";
		bool isaltreg=false;
		if (Regex.IsMatch(regs.registervalues[switchReg], "Type\\.enumIndex\\(.+\\)"))
		{
			x="switch("+regs.registervalues[switchReg].Substring(15,regs.registervalues[switchReg].Length-16)+"){\n\t";
			isaltreg=true;
		}
		for(int ind=0;ind<sinds.Length;ind++)
		{
			List<int> i=sinds[ind];
			if (i.Count != 0)
			{
				x+="case ";
				if(Utils.types[regs.lastreferencedtypes[switchReg]].definition is EnumType enumType){
					foreach(int j in i)
					{
						if (x[x.Length - 1]!=' ')
						{
							x+=" | ";
						}
						x+=Utils.stable[enumType.name]+"."+Utils.stable[enumType.constructs[j].name];
					}
				}
				else
				{
					foreach(int j in i)
					{
						if (x[x.Length - 1]!=' ')
						{
							x+=" | ";
						}
						x+=j;
					}
				}
				x+=":";
				x+=blocks[ind].Parse(regs).Replace("\n\t","\n\t\t");
				x+="\n\t";

			}
		}
		if (_default != null)
		{
			x+="default:";
			x+=_default.Parse(regs).Replace("\n\t","\n\t\t");
			x+="\n\t";
		}
		x+="}";
		return x;
	}
}
class LiftedOpsIf:LiftedOps
{
	public HLJumpOpcode condition;
	public override string Parse(DecompRegs regs)
	{
		string x="";
		string y=condition.getConditionInverted(regs);
		for(int i = 0; i < ops.Length; i++)
		{
			if (ops[i] != null)
			{
				string o=ops[i].Apply(regs);
				if (o.Length != 0)
				{
					x+="\n\t"+o+";";
				}
				/*else
				{
					x+="\n\t"+ops[i]+";;";
				}*/
			}
			else
			{
				string n=blocks[i].Parse(regs);
				x+="\n\t"+n.Replace("\n","\n\t");
			}
		}
		/*if (x.Count<char>((c)=>{return c==';';})==1){
			x="if("+y+") "+x;
		}
		else
		{*/
			x="if("+y+"){"+x+"\n}";
		//}
		return x;
	}
}
class LiftedOpsBreak:LiftedOps
{
	public override string Parse(DecompRegs regs)
	{
		return "break";
	}
}
class LiftedOpsContinue:LiftedOps
{
	public override string Parse(DecompRegs regs)
	{
		return "continue";
	}
}
class LiftedOpsIfInverted:LiftedOps
{
	public HLJumpOpcode condition;
	public override string Parse(DecompRegs regs)
	{
		string x="";
		string y=condition.getCondition(regs);
		for(int i = 0; i < ops.Length; i++)
		{
			if (ops[i] != null)
			{
				string o=ops[i].Apply(regs);
				if (o.Length != 0)
				{
					x+="\n\t"+o+";";
				}
				/*else
				{
					x+="\n\t"+ops[i]+";;";
				}*/
			}
			else
			{
				string n=blocks[i].Parse(regs);
				x+="\n\t"+n.Replace("\n","\n\t");
			}
		}
		/*if (x.Count<char>((c)=>{return c==';';})==1){
			x="if("+y+") "+x;
		}
		else
		{*/
			x="if("+y+"){"+x+"\n}";
		//}
		return x;
	}
}
class LiftedOpsIfElse:LiftedOpsIf
{
	public LiftedOps _else;
	public override string Parse(DecompRegs regs)
	{
		string x="";
		string y=condition.getConditionInverted(regs);
		for(int i = 0; i < ops.Length; i++)
		{
			if (ops[i] != null)
			{
				string o=ops[i].Apply(regs);
				if (o.Length != 0)
				{
					x+="\n\t"+o+";";
				}
				/*else
				{
					x+="\n\t"+ops[i]+";;";
				}*/
			}
			else
			{
				string n=blocks[i].Parse(regs);
				x+="\n\t"+n.Replace("\n","\n\t");
			}
		}
		if (x.Count<char>((c)=>{return c==';';})==1){
			x="if("+y+") "+x;
			if(_else is LiftedOpsIf)
			{
				x+="\nelse "+_else.Parse(regs);
			}
			else
			{
				x+="\nelse{"+_else.Parse(regs)+"\n}";
			}
		}
		else
		{
			x="if("+y+"){"+x+"\n}";
			if(_else is LiftedOpsIf)
			{
				x+="else "+_else.Parse(regs);
			}
			else
			{
				x+="else{"+_else.Parse(regs)+"\n}";
			}
		}

		return x;
	}
}
class LiftedOpsDoWhile:LiftedOps
{
	public HLJumpOpcode condition;
	public override string Parse(DecompRegs regs)
	{
		string x="";
		for(int i = 0; i < ops.Length; i++)
		{
			if (ops[i] != null)
			{
				string o=ops[i].Apply(regs);
				if (o.Length != 0)
				{
					x+="\n\t"+o+";";
				}
				else
				{
					x+="\n\t"+ops[i]+";;";
				}
			}
			else
			{
				string n=blocks[i].Parse(regs);
				x+="\n\t"+n.Replace("\n","\n\t");
			}
		}
		x="do{"+x+"\n}while("+condition.getCondition(regs)+");";
		return x;
	}
}
class Function
{
	public int type;//tindex
	public int findex;
	public int nregs;
	public int nops;
	public int[] regs;
	public HLopcode[] ops;
	public bool has_debug;
	public int? version;
	public DebugInfo? debuginfo;
	public int nassigns;
	public Tuple<int, int>[]? assigns;
	public string[] regnames;
	public static Function fromBytes(byte[] bytes, ref int loc, bool has_debug, int version)
	{
		Function self=new Function();
		self.has_debug = has_debug;
        self.version = version;
        self.type=HLbytecode.getVarInt(ref loc, bytes);
        self.findex=HLbytecode.getVarInt(ref loc, bytes);
        self.nregs=HLbytecode.getVarInt(ref loc, bytes);
        self.nops=HLbytecode.getVarInt(ref loc, bytes);
		self.regs=new int[self.nregs];
        for(int i=0;i<self.nregs;i++){
            self.regs[i]=HLbytecode.getVarInt(ref loc, bytes);
		}
		self.ops=new HLopcode[self.nops];
		HLbytecode b=new HLbytecode(){bytes=bytes};
        for(int i=0;i<self.nops;i++){
            self.ops[i]=(HLopcode.deserialize(b, ref loc));
			//Console.WriteLine(self.ops[i]);
		}
		if (bytes.Length == loc)
		{
			self.has_debug=false;
		}
        if(self.has_debug){
            self.debuginfo = DebugInfo.deserialise(bytes, self.nops, ref loc);
			if ((bytes.Length == loc)&&(self.version>=3))
			{
				self.version=-1;
			}
            if(self.version >= 3){
                self.nassigns = HLbytecode.getVarInt(ref loc, bytes);
                self.assigns = new Tuple<int, int>[self.nassigns];
                for (int i=0;i<self.nassigns;i++){
                    self.assigns[i]=(Tuple.Create(HLbytecode.getVarInt(ref loc, bytes), HLbytecode.getVarInt(ref loc, bytes)));
				}
			}
		}
		//That's the end of the regular stuff. Now, for preliminary flagging of things like for loops, break statements, etc
		for(int i = 0; i < self.ops.Length;i++)
		{
			if(self.ops[i] is HLLabelOpcode)//checking for for loops, finish later
			{
				if((i>0)&&(self.ops[i-1] is HLIntOpcode presetop))
				{
					int internalloopreg=presetop.dst;
					if(self.ops[i+1] is HLIntOpcode setlimitop)
					{
						int internalloopendreg=setlimitop.dst;
						if(self.ops[i+2] is HLJSGteOpcode breakop)
						{
							//if ((self.ops[i + 2 + breakop.offset]))
							//{
								
							//}
						}
					}
				}
			}
		}
        return self;
	}
	bool instancemethod=false;
	bool instancemethodparsed=false;
	public bool _is_instance_method()
	{
		if(instancemethodparsed==false){
			foreach(Type typ in Utils.types){
				if(typ.kind==(int)Type.Kind.OBJ){
					continue;
				}
				TypeDef t = typ.definition;
				if(t is not ObjType){
					continue;
				}
				ObjType obj = (ObjType)t;
				foreach(Tuple<int,int,int> proto in obj.protos){
					Function fn = Utils.functions[proto.Item2];
					if(fn==this){
						instancemethod=true;
						instancemethodparsed=true;
						return instancemethod;
					}
				}
			}
			instancemethod=false;
			instancemethodparsed=true;
			return instancemethod;
		}
		return instancemethod;
	}
	public bool staticmethod;
	public bool _is_this_method()
	{
		return staticmethod;
	}
	int[] reg_first_assign;
	public List<String> _usr_variable_names=new List<string>();
	public List<int> _usr_reg_indicies=new List<int>();
	public void name_locals(){
		reg_first_assign=new int[nregs];
		regnames=new string[nregs];
		for(int i = 0; i < regnames.Length; i++)
		{
			regnames[i]="var"+i;
		}
		
        List<int>[] reg_assigns=new List<int>[nregs];
        //# Register 0 is `this` in instance methods and constructors. Detect by
        //# either: the function is bound as a prototype on a class, or the
        //# function contains SetThis/GetThis opcodes (constructor).
        bool is_instance = _is_instance_method();
        bool has_this_ops =false;
		foreach(HLopcode op in this.ops)
		{
			if(op is HLSetThisOpcode)
			{
				has_this_ops=true;
			}
			if(op is HLGetThisOpcode)
			{
				has_this_ops=true;
			}
		}
        //# A constructor that only delegates to `super(...)` (no field writes of
        //# its own) has neither SetThis/GetThis ops nor a vtable proto entry
        //# (constructors aren't virtual), so it would otherwise be misdetected
        //# as a plain static function and have all its parameter names shifted
        //# by one onto the wrong registers.
        bool is_ctor_wrapper = HashlinkDecompiler.getFuncName(this).Equals("__constructor__");
        bool has_this = is_instance || has_this_ops || is_ctor_wrapper;
        if(this.has_debug&&this.assigns.Length!=0){
            foreach(Tuple<int,int> assign in this.assigns){
                //# assign: Tuple[strRef (name), VarInt (op index)]
                int val = assign.Item2 - 1;
                if(val < 0)
                    continue;
                int reg = -10395324;
                HLopcode op = this.ops[val];
				if(op is HLopcodeWithDST dstop){
                    reg = dstop.dst;
				}
                if(reg!=-10395324){
					if (reg_assigns[reg] == null)
					{
						reg_assigns[reg]=new List<int>();
					}
                    if(!reg_assigns[reg].Contains(assign.Item1)){
                        reg_assigns[reg].Add(assign.Item1);
					}
				}
			}
		}
        if(this.has_debug&&(this.assigns.Length!=0)){
            //# Assigns with op index 0 (val == -1 above) name function parameters,
            //# in order — not necessarily register 0: that's `this` for instance
            //# methods/constructors, so parameters start at register 1 there.
            int param_start = has_this?1:0;
            List<Tuple<int,int>> param_candidates = new List<Tuple<int, int>>();
			foreach(Tuple<int,int> assign in this.assigns)
			{
				if (assign.Item2 <= 0)
				{
					param_candidates.Add(assign);
				}
			}
            int param_count=-10000;
            TypeDef fun_def = Utils.types[this.type].definition;
            if(fun_def is FunType fun_fun_def){
                param_count = Math.Max(0, fun_fun_def.args.Length - param_start);
			}
            List<int> seen_param_names=new List<int>();
            int param_idx = 0;
            foreach(Tuple<int,int> assign in param_candidates){
                if((param_count!=-10000) && (param_idx >= param_count))
                    break;
                int name = assign.Item1;
                //# A body local can shadow a parameter with the same debug name
                //# (e.g. ArrayBytes.getDyn's `pos`). Keep only the first parameter
                //# use of a name and continue with the next parameter slot.
                if(seen_param_names.Contains(name))
                    continue;
                int reg = param_start + param_idx;
                if(reg >= reg_assigns.Length)
                    break;
				if (reg_assigns[reg] == null)
				{
					reg_assigns[reg]=new List<int>();
				}
                if(!reg_assigns[reg].Contains(name))
                    reg_assigns[reg].Add(name);
                //# A parameter name applies from the start of the function, even if
                //# the same register is later reassigned with the same debug name.
                reg_first_assign[reg] = -1;
                seen_param_names.Add(name);
                param_idx += 1;
			}
		}
		for(int i=0;i<regs.Length;i++){
			int _reg=regs[i];
            if((Utils.types[_reg].definition!=null) && (Utils.types[_reg].definition is VoidType)){
				if(reg_assigns[i]==null)
                    reg_assigns[i]=new List<int>();
                if(reg_assigns[i].Contains(-1)==false){
                    reg_assigns[i].Add(-1);
				}
			}
		}
        //# A register may be used as an anonymous temporary before the first
        //# debug-named assignment that names it. Naming the whole register after
        //# that later debug name makes the earlier uses look like the named
        //# variable (e.g. String.split's empty-delimiter loop bound becomes
        //# `dlen` because reg6 is later named for delimiter.length). In that
        //# case keep the pre-name segment as a temp; _check_assign will split
        //# off the named segment at the debug assignment.
        Dictionary<int,int> first_def=new Dictionary<int, int>();
		for(int op_idx=0;op_idx<ops.Length;op_idx++){
        HLopcode op=ops[op_idx];
            if(op is HLopcodeWithDST dstop){
                int reg = dstop.dst;
                if(!first_def.Keys.Contains(reg))
                    first_def[reg] = op_idx;
			}
		}
		for(int i=0;i<regnames.Length;i++){
			if (reg_assigns[i] == null)
			{
				reg_assigns[i]=new List<int>();
			}
            if(reg_assigns[i].Count!=0){
                int named_op = reg_first_assign[i];
                if((named_op>0) && (first_def.GetValueOrDefault<int,int>(i, int.MaxValue) < named_op))
                    continue;
				int sind=reg_assigns
					[i]
					[0];
                this.regnames[i] = (sind<0)?"voidReg":Utils.stable[sind];
			}
		}
        //# If the same debug name is assigned to two different registers with
        //# different types, suffix the later one so Haxe sees two distinct
        //# variables instead of a single Dynamic variable.
        Dictionary<string,List<int>> name_to_regs=new Dictionary<string, List<int>>();
		for(int i=0;i<regnames.Length;i++){
			if (!name_to_regs.Keys.Contains(regnames[i]))
			{
				name_to_regs[regnames[i]]=new List<int>();
			}
            name_to_regs[regnames[i]].Add(i);
		}
		foreach(string name in name_to_regs.Keys){
			List<int> regs=name_to_regs[name];
            if(regs.Count<=1)
                continue;
            Dictionary<int,List<int>> typed_regs=new Dictionary<int, List<int>>();
            foreach(int r in regs){
                int typ = this.regs[r];
                int typ_key = Utils.types[typ].definition.type;
				if (!typed_regs.Keys.Contains(typ_key))
				{
					typed_regs[typ_key]=new List<int>();
				}
                typed_regs[typ_key].Add(r);
			}
            //# Only rename when the same debug name is used for variables with
            //# different types.  Same-type duplicates are usually just different
            //# registers for a single source variable.
            if(typed_regs.Keys.Count <= 1){
                //# Exception: a parameter shadowed by a body local with the same
                //# name must be disambiguated (e.g. ArrayBytes.getDyn's `pos`).
				List<int> orderedb=new List<int>(regs);
				orderedb.Sort((a,b)=>(this.reg_first_assign[a])-(this.reg_first_assign[b]));
				List<int> param_regs=new List<int>();
				foreach(int r in orderedb)
				{
					if (reg_first_assign[r] == -1)
					{
						param_regs.Add(r);
					}
				}
                if(param_regs.Count!=0){
                    int suffix = 1;
                    foreach(int r in orderedb){
                        if(r == param_regs[0])
                            continue;
                        regnames[r] = name+suffix;
                        suffix += 1;
					}
				}
                continue;
			}
			List<int> ordered=new List<int>(regs);
			ordered.Sort((a,b)=>(this.reg_first_assign[a])-(this.reg_first_assign[b]));
            Dictionary<int,List<int>> by_type=new Dictionary<int, List<int>>();
            foreach(int r in ordered){
                int typ = this.regs[r];
                int typ_key = Utils.types[typ].definition.type;
				if (!by_type.Keys.Contains(typ_key))
				{
					by_type[typ_key]=new List<int>();
				}
                by_type[typ_key].Add(r);
			}
            //# Keep the earliest register of the first-seen type as the base name;
            //# rename duplicates of other types.
            List<int> kept=new List<int>();
            foreach(int r in ordered){
                int typ_key = Utils.types[this.regs[r]].definition.type;
                if(!kept.Contains(typ_key)){
                    kept.Add(typ_key);
				}else{
                    this.regnames[r] = name+kept.Count;
                    kept.Add(typ_key);
				}
			}
		}
        if((this.regnames.Length!=0)&&(this.regnames[0] == "var0")&&has_this){
            this.regnames[0] = "this";
		}
        //dbg_print("Named locals:", self.locals)
		foreach(Tuple<int,int> yty in this.assigns){
			int name_ref=yty.Item1;
			int op_idx=yty.Item2;
			string n=Utils.stable[name_ref].Trim();
			if((n.Length<=3)||(n.Substring(0,3)!="var")||(int.TryParse(n.Substring(3), out int number)==false)){
				this._usr_variable_names.Add(Utils.stable[name_ref]);
				int val = op_idx - 1;
				if((val >= 0) && (val < this.ops.Length)){
					HLopcode op = this.ops[val];
					if(op is HLopcodeWithDST dstop){
						int reg = dstop.dst;
						this._usr_reg_indicies.Add(reg);
					}
				}
			}
		}

	}
}
class Native
{
	public int lib;
	public int name;
	public int type;
	public int findex;
	public static Native fromBytes(byte[] bytes, ref int loc)
	{
		return new Native(){lib=HLbytecode.getVarInt(ref loc,bytes),name=HLbytecode.getVarInt(ref loc,bytes),type=HLbytecode.getVarInt(ref loc,bytes),findex=HLbytecode.getVarInt(ref loc,bytes)};
	}
}
class Constant
{
	public int _global;
	public int nfields;
	public int[] fields;
	public static Constant fromBytes(byte[] bytes, ref int loc)
	{
		int _global=HLbytecode.getVarInt(ref loc,bytes);
		int nfields=HLbytecode.getVarInt(ref loc,bytes);
		int[] fields=new int[nfields];
		for(int i = 0; i < nfields; i++)
		{
			fields[i]=HLbytecode.getVarInt(ref loc,bytes);
		}
		return new Constant(){_global=_global,nfields=nfields,fields=fields};
	}
}
class Utils{
	public static String[] stable=[];
	public static Double[] doubles=[];
	public static int[] ints=[];
	public static int[] globals=[];
	public static Dictionary<string,object>[] global_names=[];
	public static string[] bytes=[];
	public static Type[] types=[];
	public static Function[] functions=[];
	public static Native[] natives=[];
	public static int[] findex_map=[];
	public static Dictionary<int,Tuple<int,int,int>> _proto_map=new Dictionary<int,Tuple<int,int,int>>();
	public static Dictionary<int,Tuple<int,int>> _field_map=new Dictionary<int,Tuple<int,int>>();
	public static Constant[] constants=[];
	public static Tuple<string,int>[] enum_global_map;
	public static string GetFNamefromInd(int ind){
		return null;
	}
	public static Function f;
	public static bool isUserDefinedRegistry(int ind){
		return f._usr_reg_indicies.Contains(ind);
		/*if (f.assigns == null)
		{
			return true;
		}
		foreach(Tuple<int,int> assign in f.assigns)
		{
			if (assign.Item2 == ind)
			{
				return true;
			}
		}*/
		return true;
		

	}
	public static string getUserDefinedRegistryName(int ind){
		if (f.regnames != null)
		{
			if(f.regnames[ind]!=null)
				Console.WriteLine(ind+" is "+f.regnames[ind]);
				return f.regnames[ind];
		}
		/*if (f.assigns == null)
		{
			return "var"+ind;
		}
		foreach(Tuple<int,int> assign in f.assigns)
		{
			if (assign.Item2 == ind)
			{
				return Utils.stable[assign.Item1];
			}
		}*/
		return "var"+ind;
	}
	public static string getStringTable(int ind){
		return stable[ind];
	}
	public static string getFIndexName(int findex)
	{
		int i=Utils.findex_map[findex];
		if (i < 0)
		{
			return stable[natives[-i-1].name];
		}
		else
		{
			return HashlinkDecompiler.getFuncName(Utils.functions[Utils.findex_map[findex]]);
		}
		
	}
	public static string getGlobalName(int gindex){
		Tuple<string,int> enum_const = Utils.enum_global_map[gindex];
		if (enum_const != null)
		{
			return Utils.stable[((EnumType)Utils.types[enum_const.Item2].definition).name]+"."+enum_const.Item1;
		}
        // TODO: is this overcomplicated?
		if((gindex < 0) || (gindex >= globals.Length)){
			throw new Exception("Global "+gindex+" not found!");
		}
        if(global_names[gindex]==null){
			if(types[globals[gindex]].definition is ObjType obj_t)
			{
				return stable[obj_t.name];
			}
            throw new Exception("Global "+gindex+" does not have a constant value!");
		}
        TypeDef obj = Utils.types[Utils.globals[gindex]].definition;
        if(!(obj is ObjType)){
            throw new Exception("Global "+gindex+" is not an object!");
		}
		ObjType objobj=(ObjType)obj;
        if((Utils.stable[objobj.name]!="String")){
            throw new Exception("Global "+gindex+" is not a string! (its "+Utils.stable[objobj.type]+")");
		}
        Tuple<int,int>[] obj_fields = objobj.resolve_fields();
        if(obj_fields.Length!=2){
            throw new Exception("Global "+gindex+" seems malformed!");
		}
        object res = global_names[gindex][Utils.stable[obj_fields[0].Item1]];
		if(res is String strres)
		{
			return "\""+strres.Replace("\\","\\\\").Replace("\n","\\n").Replace("\"","\\\"").Replace("\t","\\t").Replace("\r","\\r").Replace("\a","\\a").Replace("\b","\\b").Replace("\f","\\f")+"\"";
		}
		return res.ToString();
	}
	//name,findex,pindex
	public static Tuple<int,int,int> proto_by_pindex(ObjType obj,int pindex){
        ObjType current=obj;
        List<int> visited=new List<int>();
        while((current!=null) && (!visited.Contains(current.type))){
			visited.Add(current.type);
            foreach(Tuple<int,int,int> proto in obj.protos){
                if(proto.Item3 == pindex){
                    return proto;
				}
			}
            if((current.super==null) || (current.super < 0)){
                break;
			}
			TypeDef super_def = types[current.super].definition;
			if(super_def is ObjType sobj)
			{
				current=sobj;
			}
			else
			{
				current=null;
			}
		}
        return null;
	}
	public static string getTrueFuncStringFromNameIndAndArgs(string fname,int find,string[] args)
	{
		if (find < 0)
		{
			string r=fname+"(";
			for(int i = 0; i < args.Length; i++)
			{
				if(i!=0) r+=", ";
				r+=args[i];
			}
			r+=")";
			return r;
		}
		if(!Utils.functions[find].staticmethod){
			string r=args[0]+"."+fname+"(";
			for(int i = 1; i < args.Length; i++)
			{
				if(i!=1) r+=", ";
				r+=args[i];
			}
			r+=")";
			return r;
		}
		else
		{
			string r=fname+"(";
			for(int i = 0; i < args.Length; i++)
			{
				if(i!=0) r+=", ";
				r+=args[i];
			}
			r+=")";
			return r;
		}
	}
	public static string destaticify(string fname)
	{
		string[] strings=fname.Split(".");
		if (strings[strings.Length-1].StartsWith("$"))
		{
			strings[strings.Length-1]=strings[strings.Length-1].Substring(1);
		}
		return string.Join(".",strings);
	}
}
class Type
{
	public delegate TypeDef Parser(byte[] block,ref int value);
	public static Parser[] parsers=[VoidType.fromBytes,U8Type.fromBytes,U16Type.fromBytes,I32Type.fromBytes,I64Type.fromBytes,F32Type.fromBytes,F64Type.fromBytes,BoolType.fromBytes,BytesType.fromBytes,DynType.fromBytes,FunType.fromBytes,ObjType.fromBytes,ArrayType.fromBytes,TypeType.fromBytes,RefType.fromBytes,VirtualType.fromBytes,DynObjType.fromBytes,AbstractType.fromBytes,EnumType.fromBytes,NullType.fromBytes,MethodType.fromBytes,StructType.fromBytes,PackedType.fromBytes,GUIDType.fromBytes];
	public int kind;
	public TypeDef definition;
    public enum Kind{
  VOID = 0,
        U8 = 1,
        U16 = 2,
        I32 = 3,
        I64 = 4,
        F32 = 5,
        F64 = 6,
        BOOL = 7,
        BYTES = 8,
        DYN = 9,
        FUN = 10,
        OBJ = 11,
        ARRAY = 12,
        TYPETYPE = 13,
        REF = 14,
        VIRTUAL = 15,
        DYNOBJ = 16,
        ABSTRACT = 17,
        ENUM = 18,
        NULL = 19,
        METHOD = 20,
        STRUCT = 21,
        PACKED = 22,
        GUID = 23  
    }
	public static Type fromBytes(byte[] bytes, ref int loc){
		
		int kind=HLbytecode.getVarInt(ref loc,bytes);
		return new Type(){kind=kind,definition=parsers[kind].Invoke(bytes,ref loc)};
    }
}
abstract class TypeDef
{
	public static int numTypes=0;
	public int type=numTypes++;
}
class VoidType : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new VoidType();  
    }
}
class U8Type : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new U8Type();  
    }
}
class U16Type : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new U16Type();  
    }
}
class I32Type : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new I32Type();  
    }
}
class I64Type : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new I64Type();  
    }
}
class F32Type : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new F32Type();  
    }
}
class F64Type : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new F64Type();  
    }
}
class BoolType : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new BoolType();  
    }
}
class BytesType : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new BytesType();  
    }
}
class DynType : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
	return new DynType();  
    }
}
class FunType : TypeDef{
	public int nargs;
	public int[] args;
	public int ret;
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		int nargs=HLbytecode.getVarInt(ref loc,bytes);
		int[] args=new int[nargs];
		for(int i = 0; i < nargs; i++)
		{
			args[i]=HLbytecode.getVarInt(ref loc,bytes);
		}
		int ret=HLbytecode.getVarInt(ref loc,bytes);
		return new FunType(){nargs=nargs,args=args,ret=ret};
    }
}
class ObjType : TypeDef{
	
	public int name;
	public int super;
	public int _global;
	public int nfields;
	public int nprotos;
	public int nbindings;
	public Tuple<int,int>[] fields;
	public Tuple<int,int,int>[] protos;
	public Tuple<Tuple<int,int>,int>[] bindings;
	public int[] _virtuals;
	public Dictionary<string,int> _virtual_map;
	public bool virtuals_initialized;
	public bool? _is_static;
	public ObjType? _static;
	public ObjType? _dynamic;
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		int name=HLbytecode.getVarInt(ref loc,bytes);
		int super=HLbytecode.getVarInt(ref loc,bytes);
		int global=HLbytecode.getVarInt(ref loc,bytes);
		int nfields=HLbytecode.getVarInt(ref loc,bytes);
		int nprotos=HLbytecode.getVarInt(ref loc,bytes);
		int nbindings=HLbytecode.getVarInt(ref loc,bytes);
		Tuple<int,int>[] fields=new Tuple<int,int>[nfields];
		for(int i = 0; i < nfields; i++)
		{
			fields[i]=Tuple.Create(HLbytecode.getVarInt(ref loc,bytes),HLbytecode.getVarInt(ref loc,bytes));
			//Utils._field_map.Add(fields[i].Item2,fields[i]);
		}
		Tuple<int,int,int>[] protos=new Tuple<int,int,int>[nprotos];
		for(int i = 0; i < nprotos; i++)
		{
			protos[i]=Tuple.Create(HLbytecode.getVarInt(ref loc,bytes),HLbytecode.getVarInt(ref loc,bytes),HLbytecode.getVarInt(ref loc,bytes));
			//Utils._proto_map.Add(protos[i].Item2,protos[i]);
		}
		Tuple<Tuple<int,int>,int>[] bindings=new Tuple<Tuple<int,int>,int>[nbindings];
		for(int i = 0; i < nbindings; i++)
		{
			bindings[i]=Tuple.Create(Tuple.Create(HLbytecode.getVarInt(ref loc,bytes),TypeDef.numTypes),HLbytecode.getVarInt(ref loc,bytes));
		}
		return new ObjType(){name=name,super=super,_global=global,nfields=nfields,nprotos=nprotos,nbindings=nbindings,fields=fields,protos=protos,bindings=bindings,_virtuals=null,_virtual_map=null};
    }
	public Tuple<int,int>[] resolve_fields(){

        if(this.super < 0){
            return this.fields;
		}
        Tuple<int,int>[] fields=[];
        List<int> visited_types = [];
        ObjType current_type = this;
		
        while(current_type!=null){
            if (visited_types.Contains(current_type.type))
                throw new Exception("Cyclic inheritance detected in class hierarchy.");
            visited_types.Add(current_type.type);
            fields = current_type.fields.Concat(fields).ToArray();
            if(current_type.super < 0){
                current_type = null;
			}
            else{
                TypeDef defn = Utils.types[current_type.super].definition;
                if(!(defn is ObjType)){
                    throw new Exception("Invalid superclass type.");
				}
                current_type = (ObjType)defn;
			}
		}
        return fields;
	}
}
class ArrayType : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		return new ArrayType();
    }
}
class TypeType : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		return new TypeType();
    }
}
class RefType : TypeDef{
	int type;
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		return new RefType(){type=HLbytecode.getVarInt(ref loc,bytes)};
    }
}
class VirtualType : TypeDef{
	public int nfields;
	public Tuple<int,int>[] fields;
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		int nfields=HLbytecode.getVarInt(ref loc,bytes);
		Tuple<int,int>[] fields=new Tuple<int,int>[nfields];
		for(int i = 0; i < nfields; i++)
		{
			fields[i]=Tuple.Create(HLbytecode.getVarInt(ref loc,bytes),HLbytecode.getVarInt(ref loc,bytes));
		}
		return new VirtualType(){nfields=nfields,fields=fields};
    }
}
class DynObjType : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		return new DynObjType();
    }
}
class AbstractType : TypeDef{
	public int name;
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		return new AbstractType(){name=HLbytecode.getVarInt(ref loc,bytes)};
    }
}
class EnumType : TypeDef{
	public int name;
	public int _global;
	public int nconstructs;
	public EnumConstruct[] constructs;
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		int name=HLbytecode.getVarInt(ref loc,bytes);
		int _global=HLbytecode.getVarInt(ref loc,bytes);
		int nconstructs=HLbytecode.getVarInt(ref loc,bytes);
		EnumConstruct[] _constructs=new EnumConstruct[nconstructs];
		for(int i = 0; i < nconstructs; i++)
		{
			_constructs[i]=EnumConstruct.fromBytes(bytes,ref loc);
		}
		return new EnumType(){name=name,_global=_global,nconstructs=nconstructs,constructs=_constructs};
    }
}
class EnumConstruct
{
	public int name;
	public int nparams;
	public int[] _params;
	public static EnumConstruct fromBytes(byte[] bytes, ref int loc){
		int name=HLbytecode.getVarInt(ref loc,bytes);
		int nparams=HLbytecode.getVarInt(ref loc,bytes);
		int[] _params=new int[nparams];
		for(int i = 0; i < nparams; i++)
		{
			_params[i]=HLbytecode.getVarInt(ref loc,bytes);
		}
		return new EnumConstruct(){name=name,nparams=nparams,_params=_params};
    }
}
class NullType : TypeDef{
	public int type;
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		return new NullType(){type=HLbytecode.getVarInt(ref loc,bytes)};
    }
}
class MethodType : FunType{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		int nargs=HLbytecode.getVarInt(ref loc,bytes);
		int[] args=new int[nargs];
		for(int i = 0; i < nargs; i++)
		{
			args[i]=HLbytecode.getVarInt(ref loc,bytes);
		}
		int ret=HLbytecode.getVarInt(ref loc,bytes);
		return new MethodType(){nargs=nargs,args=args,ret=ret};
    }
}
class StructType : ObjType{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		int name=HLbytecode.getVarInt(ref loc,bytes);
		int super=HLbytecode.getVarInt(ref loc,bytes);
		int global=HLbytecode.getVarInt(ref loc,bytes);
		int nfields=HLbytecode.getVarInt(ref loc,bytes);
		int nprotos=HLbytecode.getVarInt(ref loc,bytes);
		int nbindings=HLbytecode.getVarInt(ref loc,bytes);
		Tuple<int,int>[] fields=new Tuple<int,int>[nfields];
		for(int i = 0; i < nfields; i++)
		{
			fields[i]=Tuple.Create(HLbytecode.getVarInt(ref loc,bytes),HLbytecode.getVarInt(ref loc,bytes));
			//Utils._field_map.Add(fields[i].Item2,fields[i]);
		}
		Tuple<int,int,int>[] protos=new Tuple<int,int,int>[nprotos];
		for(int i = 0; i < nprotos; i++)
		{
			protos[i]=Tuple.Create(HLbytecode.getVarInt(ref loc,bytes),HLbytecode.getVarInt(ref loc,bytes),HLbytecode.getVarInt(ref loc,bytes));
			//Utils._proto_map.Add(protos[i].Item2,protos[i]);
		}
		Tuple<Tuple<int,int>,int>[] bindings=new Tuple<Tuple<int,int>,int>[nbindings];
		for(int i = 0; i < nbindings; i++)
		{
			bindings[i]=Tuple.Create(Tuple.Create(HLbytecode.getVarInt(ref loc,bytes),TypeDef.numTypes),HLbytecode.getVarInt(ref loc,bytes));
		}
		int ret=HLbytecode.getVarInt(ref loc,bytes);
		return new StructType(){name=name,super=super,_global=global,nfields=nfields,nprotos=nprotos,nbindings=nbindings,fields=fields,protos=protos,bindings=bindings,_virtuals=null,_virtual_map=null};
    }
}
class PackedType : TypeDef{
	public int type;
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		return new PackedType(){type=HLbytecode.getVarInt(ref loc,bytes)};
    }
}
class GUIDType : TypeDef{
	public static TypeDef fromBytes(byte[] bytes, ref int loc){
		return new GUIDType(){};
    }
}
class HashlinkDecompiler{
	public static unsafe String Stringify(HLbytecode c){
		return "";
	}
	public static unsafe int findEndOfLoopBlock(int loc, HLblock block, DecompRegs regs){
		return -1;
	}
	public static void Test(string file)
	{
		string s="";
		using (FileStream fs = File.Open(file, FileMode.Open))
		using (BufferedStream bs = new BufferedStream(fs))
		using (StreamReader sr = new StreamReader(bs))
		{
			s=sr.ReadToEnd();
		}
		string[] blks=s.Split(";");
		string[] strs=blks[0].Split(',');
		Console.WriteLine("loading string table");
		Utils.stable=new string[strs.Length];
		for(int i = 0; i < strs.Length; i++)
		{
			
			byte[] b=Convert.FromBase64String(strs[i]);
			Utils.stable[i]="";
			foreach(byte j in b)
			{
				Utils.stable[i]+=(char)j;
			}
		}
		strs=blks[1].Split(',');
		Utils.doubles=new double[strs.Length];
		Console.WriteLine("loading floats");
		for(int i = 0; i < strs.Length; i++)
		{
			
			byte[] b=Convert.FromBase64String(strs[i]);
			Utils.doubles[i]=BinaryPrimitives.ReadDoubleLittleEndian(b);
		}
		strs=blks[2].Split(',');
		Utils.ints=new int[strs.Length];
		Console.WriteLine("loading ints");
		for(int i = 0; i < strs.Length; i++)
		{
			
			byte[] b=Convert.FromBase64String(strs[i]);
			int j=0;
			Utils.ints[i]=HLbytecode.getVarInt(ref j,b);
			if (i < 10)
			{
				Console.WriteLine(Utils.ints[i]);
			}
		}
		strs=blks[3].Split(',');
		Utils.types=new Type[strs.Length];
		Console.WriteLine("loading types");
		for(int i = 0; i < strs.Length; i++)
		{
			
			byte[] b=Convert.FromBase64String(strs[i]);
			int j=0;
			Utils.types[i]=Type.fromBytes(b,ref j);
			
		}
		foreach(Type t in Utils.types){
            if(t.definition is ObjType){
                ObjType definition = (ObjType)t.definition;
                foreach(Tuple<int,int,int> proto in definition.protos){
                    Utils._proto_map[proto.Item2] = proto;
                    //proto_owner_map[proto.findex.value] = definition
				}
                Tuple<int,int>[] fields = definition.resolve_fields();
                foreach(Tuple<Tuple<int,int>,int> binding in definition.bindings){
                    Utils._field_map[binding.Item2] = fields[binding.Item1.Item1];
                    //field_owner_map[binding.findex.value] = definition
				}
			}
		}
		buildVirtuals();
		strs=blks[4].Split(',');
		int numfuncs=strs.Length;
		Utils.functions=new Function[strs.Length];
		Console.WriteLine("loading funcs");
		for(int i = 0; i < strs.Length; i++)
		{
			byte[] b=Convert.FromBase64String(strs[i]);
			int j=1;
			int version=HLbytecode.getVarInt(ref j,b);
			//Console.WriteLine("func "+i);
			Utils.functions[i]=Function.fromBytes(b,ref j,b[0]!='\0',version);
		}
		
		strs=blks[5].Split(',');
		Utils.globals=new int[strs.Length];
		Console.WriteLine("loading globals");
		for(int i = 0; i < strs.Length; i++)
		{
			byte[] b=Convert.FromBase64String(strs[i]);
			int j=0;
			Utils.globals[i]=HLbytecode.getVarInt(ref j, b);
		}
		strs=blks[6].Split(',');
		Console.WriteLine("loading bytes block");
		Utils.bytes=new string[strs.Length];
		for(int i = 0; i < strs.Length; i++)
		{
			
			byte[] b=Convert.FromBase64String(strs[i]);
			Utils.bytes[i]="";
			foreach(byte j in b)
			{
				Utils.bytes[i]+=(char)j;
			}
		}
		strs=blks[7].Split(',');
		Utils.findex_map=new int[strs.Length+numfuncs];
		Utils.natives=new Native[strs.Length];
		Console.WriteLine("loading natives");
		for(int i = 0; i < strs.Length; i++)
		{
			byte[] b=Convert.FromBase64String(strs[i]);
			int j=0;
			Utils.natives[i]=Native.fromBytes(b,ref j);
			Utils.findex_map[Utils.natives[i].findex]=-i-1;
		}
		for(int i = 0; i < Utils.functions.Length; i++)
		{
			Utils.findex_map[Utils.functions[i].findex]=i;
		}
		strs=blks[8].Split(',');
		Utils.constants=new Constant[strs.Length];
		Console.WriteLine("loading constants");
		for(int i = 0; i < strs.Length; i++)
		{
			byte[] b=Convert.FromBase64String(strs[i]);
			int j=0;
			Utils.constants[i]=Constant.fromBytes(b,ref j);
		}
		init_globals();
		_build_enum_global_map();
		foreach(Type t in Utils.types)
		{
			if(t.definition is ObjType objType)
			{
				
			}
		}
		map_statics();
		


















		
		/*for(int i = 0; i < Utils.functions.Length; i++)
		{*/
		for(int i=8;i<9;i++){
			DecompRegs regs=new DecompRegs(Utils.functions[i].nregs){registertypes=Utils.functions[i].regs};
			Utils.f=Utils.functions[i];
			Utils.f.name_locals();
			for(int k = 0; k < Utils.functions[i].nregs; k++)
			{
				string regname=Utils.getUserDefinedRegistryName(k);
				if (regs.registervalues.Contains(regname))//Enforces all locals having different names for certain edge cases.
				{
					int l=0;
					while (regs.registervalues.Contains(regname+l))
					{
						l=l+1;
					}
					if (Utils.f._usr_variable_names.Contains(regname)&&(!Utils.f._usr_variable_names.Contains(regname+l)))
					{
						Utils.f._usr_variable_names.Add(regname+l);
					}
					regs.registervalues[k]=regname+l;
				}else{
					regs.registervalues[k]=regname;
				}
			}
			string T=LiftedOps.fromOpcodeList(
				Utils.functions[i].ops,
				0,
				Utils.functions[i].ops.Length
				).Parse(regs);
			Console.WriteLine("func "+i);
			Console.WriteLine(getFuncNameFull(Utils.functions[i])+" { "+T+"\n}");
			for(int j = 0; j < Utils.functions[i].regnames.Length; j++)
			{
				Console.WriteLine("reg "+j+" is named "+Utils.functions[i].regnames[j]);
			}
			Console.WriteLine("func "+i+" end");
		}
		Console.WriteLine("done");
	}
	public static string getFuncName(Function f)
	{
		
        //if(f is FunctionNative):
        //    return func.name.resolve(self)
        
        if(Utils._proto_map.ContainsKey(f.findex))
            return Utils.stable[Utils._proto_map[f.findex].Item1];
        else{
			if(Utils._field_map.ContainsKey(f.findex))
                return Utils.stable[Utils._field_map[f.findex].Item1];
		}
        return "<none>";
	}
	public static string getFuncNameFull(Function f)
	{
		string sname=getFuncName(f);
		int rettype=((FunType)Utils.types[Utils.f.type].definition).ret;
		int[] argtypes=((FunType)Utils.types[Utils.f.type].definition).args;
		string[] argnames=new string[((FunType)Utils.types[Utils.f.type].definition).nargs];
		Utils.f=f;
		for(int k = 0; k < argnames.Length; k++)
		{
			argnames[k]=Utils.getUserDefinedRegistryName(k);
		}
		string output="public "+(f.staticmethod?"static ":"")+sname+"(";
		for(int i=0;i<argtypes.Length;i++){
			if (output[output.Length - 1] != '(')
			{
				output+=", ";
			}
			output+=argnames[i]+": "+typeName(Utils.types[argtypes[i]]);
		}
		output+="): "+typeName(Utils.types[rettype]);
		return output;
	}
	public static string typeName(Type type){
		TypeDef typedef = type.definition;

		if((typedef.GetType()==typeof(ObjType))&& (typedef is ObjType)){
			return Utils.stable[((ObjType)typedef).name];
		}
		else if ((typedef.GetType()==typeof(VirtualType))&& (typedef is VirtualType)){
			string[] fields = new string[((VirtualType)typedef).fields.Length];
			int i=0;
			foreach(Tuple<int,int> field in ((VirtualType)typedef).fields){
				fields[i]=Utils.stable[field.Item1];
				i++;
			}
			return "Virtual["+string.Join(", ",fields)+"]";
		}
		else if((typedef.GetType()==typeof(EnumType))&& (typedef is EnumType)){
			return Utils.stable[((EnumType)typedef).name];
		}
		return typedef.GetType().Name;
	}
	public static List<int> buildVirtuals_processed_class_ids = new List<int>();
	public static void buildVirtuals(){
        for(int i=0;i<Utils.types.Length;i++){
            buildVirtuals_processClass(i);
		}

	}
	public static Dictionary<string,int> buildVirtuals_getAllParentMethods(ObjType obj_def){
		// This helper is likely okay, but let's make it safer
		Dictionary<string,int> parent_methods = new Dictionary<string,int>();
		if (obj_def.super>0){
			try{
				int super_type = obj_def.super;
				// *** Add a check to prevent cycles in this helper too ***
				if(super_type == obj_def.type){  // Prevent self-inheritance loops
					return parent_methods;
				}
				if(Utils.types[super_type].definition is ObjType super_def){
					parent_methods=(Dictionary<string,int>)parent_methods.Concat(buildVirtuals_getAllParentMethods(super_def)).ToDictionary();
					foreach(Tuple<int,int,int> proto in super_def.protos){
						parent_methods[Utils.stable[proto.Item1]] = proto.Item2;
					}
				}
			}
			catch(IndexOutOfRangeException)
			{
				
			}
			//except (IndexError, AttributeError):
				//dbg_print(f"Warning: Could not resolve superclass for {obj_def.name.resolve(self)}")
		}
		return parent_methods;
	}
	public static void buildVirtuals_processClass(int class_id){
		Type class_type=Utils.types[class_id];
		if (buildVirtuals_processed_class_ids.Contains(class_id)){
			return;
		}

		TypeDef obj_def_general = class_type.definition;
		if(!(obj_def_general is ObjType objdef)){
			buildVirtuals_processed_class_ids.Add(class_id);
			return;
		}
		ObjType obj_def=(ObjType)obj_def_general;

		List<int> virtuals = new List<int>();
		Dictionary<string,int> virtual_map = new Dictionary<string,int>();

		if(obj_def.super>0){
			  //# virtuals and virtual_map are already empty
		}
		else{
			Type super_type;
			try{
				super_type = Utils.types[obj_def.super];
			}catch(IndexOutOfRangeException){
				return;
				//raise MalformedBytecode(
				//	f"Class '{obj_def.name.resolve(self)}' has an invalid superclass index: {obj_def.super.value}"
				//)
			}

			buildVirtuals_processClass(obj_def.super);

			if(super_type.definition is ObjType super_def){
				virtuals=(List<int>)virtuals.Concat(super_def._virtuals);
				virtual_map=(Dictionary<string,int>)virtual_map.Concat(super_def._virtual_map).ToDictionary();
			}
			else{
				//dbg_print(f"Warning: Superclass of '{obj_def.name.resolve(self)}' is not an Obj.")
			}
		}
		string[] all_parent_method_names = buildVirtuals_getAllParentMethods(objdef).Keys.ToArray();

		foreach(Tuple<int,int,int> proto in obj_def.protos){
			string method_name = Utils.stable[proto.Item1];
			int findex = proto.Item2;

			bool is_override = virtual_map.ContainsKey(method_name);
			bool is_new_virtual = ((!is_override) && all_parent_method_names.Contains(method_name));

			if(is_override){
				int vid = virtual_map[method_name];
				virtuals[vid] = findex;
			}
			else if(is_new_virtual){
				int vid = virtuals.Count;
				virtuals.Add(findex);
				virtual_map[method_name] = vid;
			}
		}
		obj_def._virtuals = virtuals.ToArray();
		obj_def._virtual_map = virtual_map;
		obj_def.virtuals_initialized = true;
		buildVirtuals_processed_class_ids.Add(class_id);
	}
	public static void init_globals(){
		List<int> keys=new List<int>();
		List<Dictionary<string,object>> values=new List<Dictionary<string,object>>();
        if(Utils.constants!=null){
            foreach(Constant _const in Utils.constants){
                Dictionary<string,object> res= new Dictionary<string,object>();
                TypeDef obj_generic = Utils.types[Utils.globals[_const._global]].definition;
                if(obj_generic is ObjType obj){
					Tuple<int,int>[] obj_fields = obj.resolve_fields();
					for(int i=0;i<_const.fields.Length;i++){
						int field=_const.fields[i];
						//# Field has:
						//# - name: strRef
						//# - type: tIndex
						//# we need to use the type to know how to resolve the const ref to the actual value
						TypeDef typ = Utils.types[obj_fields[i].Item2].definition;
						string name = Utils.stable[obj_fields[i].Item1];
						if((typ is I32Type)||(typ is U8Type)||(typ is U16Type)||(typ is I64Type)/*isinstance(typ, (I32, U8, U16, I64)*/){
							res[name] = Utils.ints[field];
						}
						else if((typ is F32Type)||(typ is F64Type)/*isinstance(typ, (F32, F64)*/){
							res[name] = Utils.doubles[field];
						}
						else if(typ is BytesType){
							res[name] = Utils.stable[field];
						}
						else{
							res[name] = field;
						}
					}
					keys.Add(_const._global);
					values.Add(res);
				
				}else
				{
					Console.WriteLine("Global was not Obj");
					throw new Exception("Global was not obj!");
				}
			}
            //assert len(final) == len(self.constants), (
            //    "Not all constants were resolved! This is often due to bad DebugInfo blocks causing buffer overrun, try passing -N to troubleshoot."
            //)
		}
		int maxv=-1;
		foreach(int i in keys){
			if (maxv < i)
			{
				maxv=i;
			}
		}
        Utils.global_names=new Dictionary<string, object>[maxv+1];
		for(int i = 0; i < keys.Count; i++)
		{
			Utils.global_names[keys[i]]=values[i];
		}
	}
	public static void _build_enum_global_map()
	{
		//	def _build_enum_global_map(code: Bytecode) -> Dict[int, Tuple[str, tIndex]]:
		/*"""
		HashLink stores parameterless enum constants as globals whose type is the
		enum type. Build a map from global index to the constructor name and enum
		type index so that `GetGlobal` can be lifted to `Red`/`Green`/... instead
		of an opaque enum-typed global object.

		The static initializer that populates these globals reads them out of the
		enum's own `__evalues__` array by construct index (`GetArray(evalues,
		Int(N))` then `SetGlobal(g, ...)`), so trace that pattern directly to
		recover the exact (global -> construct index) mapping. The order globals
		are *allocated* in (their numeric index) does not necessarily match
		declaration order — it depends on which construct is first referenced
		during compilation — so guessing via sorted-globals-zip-constructs is
		unreliable and silently mismatches names when an enum has more than one
		construct referenced across the program (see e.g. haxe.io.Error, where
		`OutsideBounds`, not `Blocked`, ends up at the lowest global index).
		"""*/
		List<int> enum_global_keys=new List<int>();
		List<List<int>> enum_global_values=new List<List<int>>();
		//enum_globals: Dict[int, List[int]] = {}
		for(int gi = 0; gi < Utils.globals.Length; gi++)
		{
			int gt=Utils.globals[gi];
			Type typ=Utils.types[gt];
			if(typ.definition is EnumType){
				if (enum_global_keys.Contains(gt))
				{
					enum_global_values[enum_global_keys.IndexOf(gt)].Add(gi);
				}else{
					enum_global_keys.Add(gt);
					enum_global_values.Add(new List<int>([gi]));
				}
			}
		}
		
			

		List<int> result_keys=new List<int>();
		List<Tuple<string,int>> result_values=new List<Tuple<string,int>>();
		List<int> resolved_globals=new List<int>();
		List<int>all_enum_globals=new List<int>();
		foreach(List<int> globals_for_type in enum_global_values){
			foreach(int i in globals_for_type){
				if(!all_enum_globals.Contains(i))
				all_enum_globals.Add(i);
			}
		}

		//# Pass 1: trace the actual `__evalues__[N]` initializer pattern.
		foreach(Function func in Utils.functions){
			//skip if native
			Dictionary<int,int> reg_int_value=new Dictionary<int,int>();
			Dictionary<int,int> reg_array_index=new Dictionary<int,int>();//  # GetArray dst -> construct index
			foreach(HLopcode op in func.ops){
				if(op is HLIntOpcode intop){
					try{
						int resolved = Utils.ints[intop.ptr];
						reg_int_value[intop.dst] = resolved;
					}
					catch
					{
						
					}
				}
				else if (op is HLGetArrayOpcode getarrop){
					int idx_reg = getarrop.index;
					if(reg_int_value.ContainsKey(idx_reg)){
						reg_array_index[getarrop.dst] = reg_int_value[idx_reg];
					}
				}
				else if (op is HLSafeCastOpcode srcop){//op.op in ("SafeCast", "UnsafeCast", "Mov"):
					int src_reg = srcop.src;
					if(reg_array_index.ContainsKey(src_reg)){
						reg_array_index[srcop.dst] = reg_array_index[src_reg];
					}
				} else if (op is HLMovOpcode srcopb){//op.op in ("SafeCast", "UnsafeCast", "Mov"):
					int src_reg = srcopb.src;
					if(reg_array_index.ContainsKey(src_reg)){
						reg_array_index[srcopb.dst] = reg_array_index[src_reg];
					}
				} else if (op is HLUnsafeCastOpcode srcopc){//op.op in ("SafeCast", "UnsafeCast", "Mov"):
					int src_reg = srcopc.src;
					if(reg_array_index.ContainsKey(src_reg)){
						reg_array_index[srcopc.dst] = reg_array_index[src_reg];
					}
				}
				else if(op is HLSetGlobalOpcode sgop){
					int gi = sgop.global;
					int src_reg = sgop.src;
					if(all_enum_globals.Contains(gi) && reg_array_index.ContainsKey(src_reg)){
						int type_idx = Utils.globals[gi];
						TypeDef obj_def = (EnumType)Utils.types[type_idx].definition;
						if(obj_def is EnumType enum_def){
							int construct_idx = reg_array_index[src_reg];
							if((0 <= construct_idx)&&(construct_idx < enum_def.constructs.Length)){
								EnumConstruct construct = enum_def.constructs[construct_idx];
								result_keys.Add(gi);
								result_values.Add(new Tuple<string,int>(Utils.stable[construct.name],Utils.globals[gi]));
								resolved_globals.Add(gi);
							}
						}
					}
				}
			}
		}
		//# Pass 2: fall back to the declaration-order guess for anything the
		//# initializer trace didn't cover (e.g. an enum with only one referenced
		//# parameterless construct, where ordering can't be ambiguous anyway).
		for(int i = 0; i < enum_global_keys.Count; i++)
		{
			int type_idx=enum_global_keys[i];
			List<int> globals_for_type=enum_global_values[i];
			TypeDef type_def = Utils.types[type_idx].definition;
			if(type_def is EnumType enum_def){
				List<int> remaining=new List<int>();
				foreach(int gi in globals_for_type)
				{
					if (!resolved_globals.Contains(gi))
					{
						remaining.Add(gi);
					}
				}
				if(remaining.Count!=0){
					remaining.Sort();
					List<EnumConstruct> parameterless = new List<EnumConstruct>();
					foreach(EnumConstruct c in enum_def.constructs)
						{
							if (c.nparams == 0)
							{
								parameterless.Add(c);
							}
						}
					List<string> used_names = new List<string>();
					foreach(int gi in globals_for_type)
					{
						if (result_keys.Contains(gi))
						{
							used_names.Add(result_values[result_keys.IndexOf(gi)].Item1);
						}
					}
					List<EnumConstruct> remaining_constructs=new List<EnumConstruct>();
					foreach(EnumConstruct c in parameterless)
					{
						if (!used_names.Contains(Utils.stable[c.name]))
						{
							remaining_constructs.Add(c);
						}
					}
					for(int j = 0; j < remaining.Count; j++)
					{
						int gi=remaining[j];
						EnumConstruct construct=remaining_constructs[j];
						if (result_keys.Contains(gi))
						{
							result_values[result_keys.IndexOf(gi)]=new Tuple<string, int>(Utils.stable[construct.name],Utils.globals[gi]);
						}
						else
						{
							result_keys.Add(gi);
							result_values.Add(new Tuple<string, int>(Utils.stable[construct.name],Utils.globals[gi]));
						}
					}
						
				}
			}
		}
		int maxkey=-1;
		foreach(int key in result_keys)
		{
			if (key > maxkey)
			{
				maxkey=key;
			}
		}
		Utils.enum_global_map=new Tuple<string, int>[maxkey+1];
		for(int i = 0; i < result_keys.Count; i++)
		{
			Utils.enum_global_map[result_keys[i]]=result_values[i];
		}
	}
	public static void map_statics(){
        //"""
        //Maps Haxe compiler-generated static Obj types to their dynamic counterparts, and vice versa. This makes pairing sets of Obj datastructures into full decompiled class definitions much easier later on.
        //"""
		Dictionary<string,List<ObjType>> names=new Dictionary<string, List<ObjType>>();
        List<ObjType> objs=new List<ObjType>();
		foreach(Type typ in Utils.types){
			if(typ.definition is ObjType objt)
				if(objt.name>0)
	                objs.Add(objt);
		}

        foreach(ObjType obj in objs)
            names[Utils.destaticify(Utils.stable[obj.name])] = new List<ObjType>();
        foreach(ObjType obj in objs)
            names[Utils.destaticify(Utils.stable[obj.name])].Add(obj);

        foreach(List<ObjType> v in names.Values){
			if (v.Count > 2)
			{
				throw new Exception("There should only be two matching Objs for each name!");
			}
			ObjType s=null;
			ObjType d=null;
            foreach(ObjType obj in v){
                if(Utils.stable[obj.name]!=Utils.destaticify(Utils.stable[obj.name])){
                    if(s!=null)
                        throw new Exception("Duplicate static classes!");
                    s = obj;
				}
                else{
                    if(d!=null)
                        throw new Exception("Duplicate dynamic classes!");
                    d = obj;
				}
			}
            if(s!=null){
                s._is_static = true;
                if(d!=null)
                    s._dynamic = d;
			}
            if(d!=null){
                d._is_static = false;
                if(s!=null)
                    d._static = s;
			}
		}
		foreach(ObjType obj in objs)
		{
			foreach(Tuple<int,int,int> proto in obj.protos){
				if(Utils.findex_map[proto.Item2]>=0)
				Utils.functions[Utils.findex_map[proto.Item2]].staticmethod=obj._is_static.Value;
			}
			foreach(Tuple<Tuple<int,int>,int> binding in obj.bindings){
				if(Utils.findex_map[binding.Item2]>=0)
				Utils.functions[Utils.findex_map[binding.Item2]].staticmethod=obj._is_static.Value;
			}
		}
	}
}
