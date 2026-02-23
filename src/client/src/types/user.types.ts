export interface CreateUserDto {
  username: string;
  fullName: string;
  fullNameEn?: string;
  password: string;
  role: string;
  department: string;
}
