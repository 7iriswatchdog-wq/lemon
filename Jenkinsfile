pipeline {
    agent any

    environment {
        SCANNER_HOME = tool 'SonarScanner'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build') {
            steps {
                echo "Building branch: ${env.BRANCH_NAME}"
            }
        }

       stage('SonarQube Analysis') {
    steps {
        withSonarQubeEnv('SonarQube') {
            sh """
                ${SCANNER_HOME}/bin/sonar-scanner \
                -Dsonar.projectKey=lemon-${env.BRANCH_NAME} \
                -Dsonar.projectName=lemon-${env.BRANCH_NAME} \
                -Dsonar.sources=.
            """
        }
    }
}

    }
}
