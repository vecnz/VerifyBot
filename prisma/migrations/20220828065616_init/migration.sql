/*
  Warnings:

  - You are about to drop the column `server_role` on the `Server` table. All the data in the column will be lost.
  - You are about to drop the column `student_role` on the `Server` table. All the data in the column will be lost.
  - Added the required column `staffRole` to the `Server` table without a default value. This is not possible if the table is not empty.
  - Added the required column `studentRole` to the `Server` table without a default value. This is not possible if the table is not empty.

*/
-- AlterTable
ALTER TABLE "Server" DROP COLUMN "server_role",
DROP COLUMN "student_role",
ADD COLUMN     "staffRole" TEXT NOT NULL,
ADD COLUMN     "studentRole" TEXT NOT NULL;
