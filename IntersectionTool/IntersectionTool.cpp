﻿#define Tool _declspec(dllexport)

extern "C" {

	/********************************************************/
/* AABB-triangle overlap test code                      */
/* by Tomas Möller                                      */
/* Function: int triBoxOverlap(float boxcenter[3],      */
/*          float boxhalfsize[3],float triverts[3][3]); */
/* History:                                             */
/*   2001-03-05: released the code in its first version */
/*                                                      */
/* Acknowledgement: Many thanks to Pierre Terdiman for  */
/* suggestions and discussions on how to optimize code. */
/* Thanks to David Hunt for finding a ">="-bug!         */
/********************************************************/
/*
Copyright 2020 Tomas Akenine-Möller

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#include <math.h>
#include <stdio.h>

#define X 0
#define Y 1
#define Z 2

#define CROSS(dest,v1,v2) \
          dest[0]=v1[1]*v2[2]-v1[2]*v2[1]; \
          dest[1]=v1[2]*v2[0]-v1[0]*v2[2]; \
          dest[2]=v1[0]*v2[1]-v1[1]*v2[0];

#define DOT(v1,v2) (v1[0]*v2[0]+v1[1]*v2[1]+v1[2]*v2[2])

#define SUB(dest,v1,v2) \
          dest[0]=v1[0]-v2[0]; \
          dest[1]=v1[1]-v2[1]; \
          dest[2]=v1[2]-v2[2];

#define FINDMINMAX(x0,x1,x2,min,max) \
  min = max = x0;   \
  if(x1<min) min=x1;\
  if(x1>max) max=x1;\
  if(x2<min) min=x2;\
  if(x2>max) max=x2;

	int planeBoxOverlap(float normal[3], float d, float maxbox[3])
	{
		int q;
		float vmin[3], vmax[3];
		for (q = X; q <= Z; q++)
		{
			if (normal[q] > 0.0f)
			{
				vmin[q] = -maxbox[q];
				vmax[q] = maxbox[q];
			}
			else
			{
				vmin[q] = maxbox[q];
				vmax[q] = -maxbox[q];
			}
		}
		if (DOT(normal, vmin) + d > 0.0f) return 0;
		if (DOT(normal, vmax) + d >= 0.0f) return 1;

		return 0;
	}


	/*======================== X-tests ========================*/
#define AXISTEST_X01(a, b, fa, fb)			   \
	p0 = a*v0[Y] - b*v0[Z];			       	   \
	p2 = a*v2[Y] - b*v2[Z];			       	   \
        if(p0<p2) {min=p0; max=p2;} else {min=p2; max=p0;} \
	rad = fa * boxhalfsize[Y] + fb * boxhalfsize[Z];   \
	if(min>rad || max<-rad) return 0;

#define AXISTEST_X2(a, b, fa, fb)			   \
	p0 = a*v0[Y] - b*v0[Z];			           \
	p1 = a*v1[Y] - b*v1[Z];			       	   \
        if(p0<p1) {min=p0; max=p1;} else {min=p1; max=p0;} \
	rad = fa * boxhalfsize[Y] + fb * boxhalfsize[Z];   \
	if(min>rad || max<-rad) return 0;

/*======================== Y-tests ========================*/
#define AXISTEST_Y02(a, b, fa, fb)			   \
	p0 = -a*v0[X] + b*v0[Z];		      	   \
	p2 = -a*v2[X] + b*v2[Z];	       	       	   \
        if(p0<p2) {min=p0; max=p2;} else {min=p2; max=p0;} \
	rad = fa * boxhalfsize[X] + fb * boxhalfsize[Z];   \
	if(min>rad || max<-rad) return 0;

#define AXISTEST_Y1(a, b, fa, fb)			   \
	p0 = -a*v0[X] + b*v0[Z];		      	   \
	p1 = -a*v1[X] + b*v1[Z];	     	       	   \
        if(p0<p1) {min=p0; max=p1;} else {min=p1; max=p0;} \
	rad = fa * boxhalfsize[X] + fb * boxhalfsize[Z];   \
	if(min>rad || max<-rad) return 0;

/*======================== Z-tests ========================*/

#define AXISTEST_Z12(a, b, fa, fb)			   \
	p1 = a*v1[X] - b*v1[Y];			           \
	p2 = a*v2[X] - b*v2[Y];			       	   \
        if(p2<p1) {min=p2; max=p1;} else {min=p1; max=p2;} \
	rad = fa * boxhalfsize[X] + fb * boxhalfsize[Y];   \
	if(min>rad || max<-rad) return 0;

#define AXISTEST_Z0(a, b, fa, fb)			   \
	p0 = a*v0[X] - b*v0[Y];				   \
	p1 = a*v1[X] - b*v1[Y];			           \
        if(p0<p1) {min=p0; max=p1;} else {min=p1; max=p0;} \
	rad = fa * boxhalfsize[X] + fb * boxhalfsize[Y];   \
	if(min>rad || max<-rad) return 0;

	int triBoxOverlap(float boxcenter[3], float boxhalfsize[3], float triverts[3][3])
	{

		/*    use separating axis theorem to test overlap between triangle and box */
		/*    need to test for overlap in these directions: */
		/*    1) the {x,y,z}-directions (actually, since we use the AABB of the triangle */
		/*       we do not even need to test these) */
		/*    2) normal of the triangle */
		/*    3) crossproduct(edge from tri, {x,y,z}-directin) */
		/*       this gives 3x3=9 more tests */
		float v0[3], v1[3], v2[3];
		float axis[3];
		float min, max, d, p0, p1, p2, rad, fex, fey, fez;
		float normal[3], e0[3], e1[3], e2[3];

		/* 1) first test overlap in the {x,y,z}-directions */
		/*    find min, max of the triangle each direction, and test for overlap in */
		/*    that direction -- this is equivalent to testing a minimal AABB around */
		/*    the triangle against the AABB */
#if 1
   /* This is the fastest branch on Sun */
   /* move everything so that the boxcenter is in (0,0,0) */
		SUB(v0, triverts[0], boxcenter);
		SUB(v1, triverts[1], boxcenter);
		SUB(v2, triverts[2], boxcenter);

		/* test in X-direction */
		FINDMINMAX(v0[X], v1[X], v2[X], min, max);
		if (min > boxhalfsize[X] || max < -boxhalfsize[X]) return 0;

		/* test in Y-direction */
		FINDMINMAX(v0[Y], v1[Y], v2[Y], min, max);
		if (min > boxhalfsize[Y] || max < -boxhalfsize[Y]) return 0;

		/* test in Z-direction */
		FINDMINMAX(v0[Z], v1[Z], v2[Z], min, max);
		if (min > boxhalfsize[Z] || max < -boxhalfsize[Z]) return 0;
#else
   /*    another implementation */
   /*    test in X */
		v0[X] = triverts[0][X] - boxcenter[X];
		v1[X] = triverts[1][X] - boxcenter[X];
		v2[X] = triverts[2][X] - boxcenter[X];
		FINDMINMAX(v0[X], v1[X], v2[X], min, max);
		if (min > boxhalfsize[X] || max < -boxhalfsize[X]) return 0;

		/*    test in Y */
		v0[Y] = triverts[0][Y] - boxcenter[Y];
		v1[Y] = triverts[1][Y] - boxcenter[Y];
		v2[Y] = triverts[2][Y] - boxcenter[Y];
		FINDMINMAX(v0[Y], v1[Y], v2[Y], min, max);
		if (min > boxhalfsize[Y] || max < -boxhalfsize[Y]) return 0;

		/*    test in Z */
		v0[Z] = triverts[0][Z] - boxcenter[Z];
		v1[Z] = triverts[1][Z] - boxcenter[Z];
		v2[Z] = triverts[2][Z] - boxcenter[Z];
		FINDMINMAX(v0[Z], v1[Z], v2[Z], min, max);
		if (min > boxhalfsize[Z] || max < -boxhalfsize[Z]) return 0;
#endif

		/*    2) */
		/*    test if the box intersects the plane of the triangle */
		/*    compute plane equation of triangle: normal*x+d=0 */
		SUB(e0, v1, v0);      /* tri edge 0 */
		SUB(e1, v2, v1);      /* tri edge 1 */
		CROSS(normal, e0, e1);
		d = -DOT(normal, v0);  /* plane eq: normal.x+d=0 */

		if (!planeBoxOverlap(normal, d, boxhalfsize)) return 0;

		/*    compute the last triangle edge */
		SUB(e2, v0, v2);

		/*    3) */
		fex = fabs(e0[X]);
		fey = fabs(e0[Y]);
		fez = fabs(e0[Z]);
		AXISTEST_X01(e0[Z], e0[Y], fez, fey);
		AXISTEST_Y02(e0[Z], e0[X], fez, fex);
		AXISTEST_Z12(e0[Y], e0[X], fey, fex);

		fex = fabs(e1[X]);
		fey = fabs(e1[Y]);
		fez = fabs(e1[Z]);
		AXISTEST_X01(e1[Z], e1[Y], fez, fey);
		AXISTEST_Y02(e1[Z], e1[X], fez, fex);
		AXISTEST_Z0(e1[Y], e1[X], fey, fex);


		fex = fabs(e2[X]);
		fey = fabs(e2[Y]);
		fez = fabs(e2[Z]);
		AXISTEST_X2(e2[Z], e2[Y], fez, fey);
		AXISTEST_Y1(e2[Z], e2[X], fez, fex);
		AXISTEST_Z12(e2[Y], e2[X], fey, fex);

		return 1;
	}
	Tool struct Point
	{
		float x;
		float y;
		float z;
	};
	Tool int Intersection(Point voxel, Point Triangle1, Point Triangle2, Point Triangle3) {
		float boxcenter[3];
		boxcenter[0] = voxel.x;
		boxcenter[1] = voxel.y;
		boxcenter[2] = voxel.z;
		float boxhalfsize[3];
		boxhalfsize[0] = 0.5;
		boxhalfsize[1] = 0.5;
		boxhalfsize[2] = 0.5;
		float triverts[3][3];
		triverts[0][0] = Triangle1.x;
		triverts[0][1] = Triangle1.y;
		triverts[0][2] = Triangle1.z;
		triverts[1][0] = Triangle2.x;
		triverts[1][1] = Triangle2.y;
		triverts[1][2] = Triangle2.z;
		triverts[2][0] = Triangle3.x;
		triverts[2][1] = Triangle3.y;
		triverts[2][2] = Triangle3.z;
		
		return triBoxOverlap(boxcenter, boxhalfsize, triverts);
	}
}