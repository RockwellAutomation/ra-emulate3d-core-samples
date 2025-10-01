"""
==============================================================================  
 MIT License  
  
 Copyright (c) 2023 Rockwell Automation, Inc.  
  
 Permission is hereby granted, free of charge, to any person obtaining a copy  
 of this software and associated documentation files (the "Software"), to deal  
 in the Software without restriction, including without limitation the rights  
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell  
 copies of the Software, and to permit persons to whom the Software is  
 furnished to do so, subject to the following conditions:  
  
 The above copyright notice and this permission notice shall be included in all  
 copies or substantial portions of the Software.  
  
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR  
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,  
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE  
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER  
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE  
 SOFTWARE.    
==============================================================================
"""

#An object which has a string property
class SimplePython:
	def __init__(self):
		self.mystring = 'Hello from Python Object'
		self.time = 0;
	def getMyString(self):
		return self.mystring
	def setMyString(self, newString):
		self.mystring = newString
	def getTime(self):
		return self.time
	def setTime(self, newTime):
		self.time = newTime

#return an instatiation of a simple object
def getSimpleObject():
	return SimplePython()
