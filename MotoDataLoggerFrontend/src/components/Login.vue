<template>
    <div class="login-container">
      <h2>Login</h2>
      <form @submit.prevent="handleSubmit">
        <div class="form-group">
          <label for="username">Username</label>
          <input type="text" id="username" v-model="username" required />
        </div>
        <div class="form-group">
          <label for="password">Password</label>
          <input type="password" id="password" v-model="password" required />
        </div>
        <button type="submit" :disabled="isSubmitting">
          <span v-if="!isSubmitting">Login</span>
          <span v-else>Logging in...</span>
        </button>
        <div v-if="loginError" class="error-message">
           {{ loginError }}
       </div>
      </form>
    </div>
  </template>
  
  <script>
  import axios from 'axios';
  
  export default {
    name: 'Login',
    data() {
      return {
        username: '',
        password: '',
        isSubmitting: false,
        loginError: null,
      };
    },
    methods: {
      async handleSubmit() {
        this.isSubmitting = true;
        this.loginError = null;
        try {
          const response = await axios.post('http://localhost:5000/api/auth/login', { // Replace with your API endpoint
            username: this.username,
            password: this.password,
          });
          
          const token = response.data.token;
          localStorage.setItem('token', token);
          
          //redirect user to main page after successful login
          this.$router.push('/');
        } catch (error) {
          console.error('Login failed:', error);
          if (error.response && error.response.data && error.response.data.error){
             this.loginError = error.response.data.error;
          }else if (error.response && error.response.data && error.response.data.title){
            this.loginError = error.response.data.title;
          }
          else {
             this.loginError = 'Login failed. Please check your credentials.';
          }
        } finally {
          this.isSubmitting = false;
        }
      },
    },
  };
  </script>
  
  <style scoped>
  .login-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100vh;
    width: 100vw;
    background-color: #f0f0f0;
  }
  .form-group {
   margin-bottom: 10px;
   display: flex;
   flex-direction: column;
  }
  .error-message{
   color: red;
  }
  </style>
  